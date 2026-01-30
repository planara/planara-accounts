using Microsoft.EntityFrameworkCore;
using Npgsql;
using Planara.Accounts.Data;
using Planara.Accounts.Data.Domain;
using Planara.Common.Kafka;
using Planara.Kafka.Exceptions;
using Planara.Kafka.Interfaces;

namespace Planara.Accounts.Workers;

public class KafkaConsumerWorker(
    ILogger<KafkaConsumerWorker> logger,
    IKafkaConsumer<UserCreatedMessage> consumer,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka background service started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await consumer.ConsumeAsync("Auth", cancellationToken);
                
                if (result?.Message?.Value is null) continue;

                var message = result.Message.Value;
                
                logger.LogInformation(
                    "Kafka: message received from Auth: userId -> {Id}, email -> {Message}",
                    message.UserId, message.Email);
                
                using var scope = scopeFactory.CreateScope();
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                
                try
                {
                    var username = message.Email.Split('@')[0];
                    dataContext.Profiles.Add(new Profile
                    {
                        UserId = message.UserId,
                        DisplayName = username,
                        Username = username,
                        Name = null,
                        Surname = null,
                        AvatarUrl = null,
                        Bio = null
                    });

                    await dataContext.SaveChangesAsync(cancellationToken);
                    
                    await consumer.CommitAsync(result, cancellationToken);
                }
                catch (DbUpdateException e) when (IsUniqueViolation(e))
                {
                    logger.LogInformation("Profile already exists for userId={UserId}. Skipping.", message.UserId);
                    await consumer.CommitAsync(result, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Kafka consumer cancellation requested.");
                break;
            }
            catch (KafkaConsumeException ex)
            {
                logger.LogError(ex, "Failed to consume Kafka message");
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while consuming Kafka message.");
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        try
        {
            consumer.Close();
            logger.LogInformation("Kafka consumer closed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error closing Kafka consumer.");
        }
    }
    
    /// <summary>
    /// Проверка исключения на нарушение правила индекса IsUnique
    /// "23505" код ошибки postgres
    /// </summary>
    /// <param name="e">Исключение</param>
    private static bool IsUniqueViolation(DbUpdateException e)
        => e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}