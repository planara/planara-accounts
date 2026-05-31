using Confluent.Kafka;
using Planara.Common.Kafka;
using Planara.Kafka.Interfaces;

namespace Planara.Accounts.Tests.Fakes;

public class FakeKafkaConsumer : IKafkaConsumer<UserCreatedMessage>
{
    public Queue<ConsumeResult<string, UserCreatedMessage>?> Results { get; } = new();

    public List<ConsumeResult<string, UserCreatedMessage>> Committed { get; } = [];

    public bool Closed { get; private set; }

    public void Reset()
    {
        Results.Clear();
        Committed.Clear();
        Closed = false;
    }

    public Task<ConsumeResult<string, UserCreatedMessage>?> ConsumeAsync(
        string topicKey,
        CancellationToken cancellationToken = default)
    {
        if (Results.Count == 0)
            return Task.FromResult<ConsumeResult<string, UserCreatedMessage>?>(null);

        return Task.FromResult(Results.Dequeue());
    }

    public Task CommitAsync(
        ConsumeResult<string, UserCreatedMessage> result,
        CancellationToken cancellationToken = default)
    {
        Committed.Add(result);
        return Task.CompletedTask;
    }

    public void Close()
    {
        Closed = true;
    }

    public static ConsumeResult<string, UserCreatedMessage> CreateResult(UserCreatedMessage message)
    {
        return new ConsumeResult<string, UserCreatedMessage>
        {
            Message = new Message<string, UserCreatedMessage>
            {
                Key = message.UserId.ToString("N"),
                Value = message
            }
        };
    }

    public static ConsumeResult<string, UserCreatedMessage> CreateNullMessageResult()
    {
        return new ConsumeResult<string, UserCreatedMessage>
        {
            Message = new Message<string, UserCreatedMessage>
            {
                Key = "null",
                Value = null!
            }
        };
    }
    
    public static ConsumeResult<string, UserCreatedMessage> CreateNullInnerMessageResult()
    {
        return new ConsumeResult<string, UserCreatedMessage>
        {
            Message = null!
        };
    }
}