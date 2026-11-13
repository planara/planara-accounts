using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Planara.Accounts.Data;
using Planara.Accounts.Data.Domain;
using Planara.Common.Kafka;
using Planara.Common.Kafka.Messages.Auth;
using Planara.Kafka.Interfaces;

namespace Planara.Accounts.Workers;

public class UserCreatedKafkaConsumerWorker(
    ILogger<UserCreatedKafkaConsumerWorker> logger,
    IKafkaConsumer<UserCreatedMessage> consumer,
    IServiceScopeFactory scopeFactory)
    : KafkaConsumerWorkerBase<UserCreatedMessage>(logger, consumer, scopeFactory)
{
    protected override string TopicKey => KafkaTopicKeys.UserCreated;

    protected override async Task HandleMessage(
        UserCreatedMessage message, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        logger.LogInformation( "Kafka: user created message received: userId - {UserId}, email - {Email}", 
            message.UserId, message.Email);

        var dataContext = serviceProvider.GetRequiredService<DataContext>();

        try
        {
            var displayName = message.Email.Split('@')[0];
            
            dataContext.Profiles.Add(new Profile
            {
                UserId = message.UserId,
                DisplayName = displayName,
                Username = $"user_{message.UserId:N}",
                Name = message.Name,
                Surname = message.Surname,
                AvatarUrl = null,
                Bio = null
            });

            await dataContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (IsUniqueViolation(e))
        {
            logger.LogInformation( "Profile already exists for userId={UserId}. Skipping.", message.UserId);
        }
    }

    /// <summary>
    /// Проверка исключения на нарушение правила индекса IsUnique.
    /// "23505" — код ошибки PostgreSQL.
    /// </summary>
    /// <param name="e">Исключение</param>
    [ExcludeFromCodeCoverage]
    private static bool IsUniqueViolation(DbUpdateException e)
        => e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}