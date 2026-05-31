using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Planara.Accounts.Data.Domain;
using Planara.Accounts.Tests.Fakes;
using Planara.Accounts.Workers;
using Planara.Common.Kafka;

namespace Planara.Accounts.Tests.Api;

public class KafkaConsumerWorkerTests : BaseApiTest
{
    public KafkaConsumerWorkerTests(ApiTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ConsumeOnce_ValidMessage_CreatesProfile_AndCommits()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        var userId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        
        var fake = scope.ServiceProvider.GetRequiredService<FakeKafkaConsumer>();
        fake.Reset();
        
        var worker = scope.ServiceProvider.GetRequiredService<KafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer.CreateResult(new UserCreatedMessage
        {
            UserId = userId,
            Email = "test@planara.local"
        }));

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().HaveCount(1);

        Context.ChangeTracker.Clear();

        var profile = await Context.Profiles
            .AsNoTracking()
            .SingleAsync(x => x.UserId == userId);

        profile.Username.Should().Be("test");
        profile.DisplayName.Should().Be("test");
        profile.Name.Should().BeNull();
        profile.Surname.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
        profile.Bio.Should().BeNull();
    }

    [Fact]
    public async Task ConsumeOnce_NullMessage_DoesNotCreateProfile_AndDoesNotCommit()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();
        
        var fake = scope.ServiceProvider.GetRequiredService<FakeKafkaConsumer>();
        fake.Reset();
        
        var worker = scope.ServiceProvider.GetRequiredService<KafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer.CreateNullMessageResult());

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().BeEmpty();

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task ConsumeOnce_WhenProfileAlreadyExists_Commits_AndDoesNotCreateDuplicate()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);
        
        using var scope = Factory.Services.CreateScope();
        
        var fake = scope.ServiceProvider.GetRequiredService<FakeKafkaConsumer>();
        fake.Reset();

        var userId = Guid.NewGuid();

        Context.Profiles.Add(new Profile
        {
            UserId = userId,
            Username = "existing",
            DisplayName = "Existing",
            Name = null,
            Surname = null,
            AvatarUrl = null,
            Bio = null
        });

        await Context.SaveChangesAsync();
        
        var worker = scope.ServiceProvider.GetRequiredService<KafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer.CreateResult(new UserCreatedMessage
        {
            UserId = userId,
            Email = "test@planara.local"
        }));

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().HaveCount(1);

        Context.ChangeTracker.Clear();

        var profiles = await Context.Profiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToArrayAsync();

        profiles.Should().HaveCount(1);
        profiles[0].Username.Should().Be("existing");
        profiles[0].DisplayName.Should().Be("Existing");
    }

    [Fact]
    public async Task ConsumeOnce_WhenNoMessages_DoesNothing()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();
        
        var fake = scope.ServiceProvider.GetRequiredService<FakeKafkaConsumer>();
        fake.Reset();
        
        var worker = scope.ServiceProvider.GetRequiredService<KafkaConsumerWorker>();

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().BeEmpty();

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }
    
    [Fact]
    public async Task ConsumeOnce_NullInnerMessage_DoesNotCreateProfile_AndDoesNotCommit()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider.GetRequiredService<FakeKafkaConsumer>();
        fake.Reset();

        var worker = scope.ServiceProvider.GetRequiredService<KafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer.CreateNullInnerMessageResult());

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().BeEmpty();

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }
}