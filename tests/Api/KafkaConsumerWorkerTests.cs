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
    public async Task UserCreatedConsumer_ConsumeOnce_ValidMessage_CreatesProfile_AndCommits()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        var userId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserCreatedMessage>>();

        fake.Reset();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserCreatedKafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer<UserCreatedMessage>.CreateResult(
            new UserCreatedMessage
            {
                UserId = userId,
                Email = "test@planara.local"
            }));

        await worker.ConsumeOnce(CancellationToken.None);

        fake.ConsumedTopicKeys.Should().ContainSingle()
            .Which.Should().Be(KafkaTopicKeys.UserCreated);

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
    public async Task UserCreatedConsumer_ConsumeOnce_NullResult_DoesNotCreateProfile_AndDoesNotCommit()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserCreatedMessage>>();

        fake.Reset();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserCreatedKafkaConsumerWorker>();

        await worker.ConsumeOnce(CancellationToken.None);

        fake.ConsumedTopicKeys.Should().ContainSingle()
            .Which.Should().Be(KafkaTopicKeys.UserCreated);

        fake.Committed.Should().BeEmpty();

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task UserCreatedConsumer_ConsumeOnce_NullMessage_DoesNotCreateProfile_AndDoesNotCommit()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserCreatedMessage>>();

        fake.Reset();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserCreatedKafkaConsumerWorker>();

        fake.Results.Enqueue(
            FakeKafkaConsumer<UserCreatedMessage>.CreateNullMessageResult());

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().BeEmpty();

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task UserCreatedConsumer_ConsumeOnce_NullInnerMessage_DoesNotCreateProfile_AndDoesNotCommit()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserCreatedMessage>>();

        fake.Reset();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserCreatedKafkaConsumerWorker>();

        fake.Results.Enqueue(
            FakeKafkaConsumer<UserCreatedMessage>.CreateNullInnerMessageResult());

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().BeEmpty();

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task UserCreatedConsumer_ConsumeOnce_WhenProfileAlreadyExists_Commits_AndDoesNotCreateDuplicate()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserCreatedMessage>>();

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

        var worker = scope.ServiceProvider
            .GetRequiredService<UserCreatedKafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer<UserCreatedMessage>.CreateResult(
            new UserCreatedMessage
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
    public async Task UserDeletedConsumer_ConsumeOnce_ExistingProfile_DeletesProfile_AndCommits()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserDeletedMessage>>();

        fake.Reset();

        var userId = Guid.NewGuid();

        Context.Profiles.Add(new Profile
        {
            UserId = userId,
            Username = "delete-me",
            DisplayName = "Delete Me",
            Name = null,
            Surname = null,
            AvatarUrl = null,
            Bio = null
        });

        await Context.SaveChangesAsync();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserDeletedKafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer<UserDeletedMessage>.CreateResult(
            new UserDeletedMessage
            {
                UserId = userId
            }));

        await worker.ConsumeOnce(CancellationToken.None);

        fake.ConsumedTopicKeys.Should().ContainSingle()
            .Which.Should().Be(KafkaTopicKeys.UserDeleted);

        fake.Committed.Should().HaveCount(1);

        Context.ChangeTracker.Clear();

        var exists = await Context.Profiles
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UserDeletedConsumer_ConsumeOnce_WhenProfileDoesNotExist_Commits()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserDeletedMessage>>();

        fake.Reset();

        var userId = Guid.NewGuid();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserDeletedKafkaConsumerWorker>();

        fake.Results.Enqueue(FakeKafkaConsumer<UserDeletedMessage>.CreateResult(
            new UserDeletedMessage
            {
                UserId = userId
            }));

        await worker.ConsumeOnce(CancellationToken.None);

        fake.Committed.Should().HaveCount(1);

        var count = await Context.Profiles.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task UserDeletedConsumer_ConsumeOnce_NullResult_DoesNotCommit()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        using var scope = Factory.Services.CreateScope();

        var fake = scope.ServiceProvider
            .GetRequiredService<FakeKafkaConsumer<UserDeletedMessage>>();

        fake.Reset();

        var worker = scope.ServiceProvider
            .GetRequiredService<UserDeletedKafkaConsumerWorker>();

        await worker.ConsumeOnce(CancellationToken.None);

        fake.ConsumedTopicKeys.Should().ContainSingle()
            .Which.Should().Be(KafkaTopicKeys.UserDeleted);

        fake.Committed.Should().BeEmpty();
    }
}