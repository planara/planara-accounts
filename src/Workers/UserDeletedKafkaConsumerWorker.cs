using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data;
using Planara.Common.Kafka;
using Planara.Kafka.Interfaces;

namespace Planara.Accounts.Workers;

public class UserDeletedKafkaConsumerWorker(
    ILogger<UserDeletedKafkaConsumerWorker> logger,
    IKafkaConsumer<UserDeletedMessage> consumer,
    IServiceScopeFactory scopeFactory)
    : KafkaConsumerWorkerBase<UserDeletedMessage>(logger, consumer, scopeFactory)
{
    protected override string TopicKey => KafkaTopicKeys.UserDeleted;

    protected override async Task HandleMessage(
        UserDeletedMessage message,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka: user deleted message received: userId - {UserId}", 
            message.UserId);

        var dataContext = serviceProvider.GetRequiredService<DataContext>();

        var deletedCount = await dataContext.Profiles
            .Where(x => x.UserId == message.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation(
            "Profile deletion completed for userId={UserId}. Deleted rows: {DeletedCount}",
            message.UserId, deletedCount);
    }
}
