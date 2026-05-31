using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data.Domain;

namespace Planara.Accounts.Tests.Api;

public class MutationTests : BaseApiTest
{
    public MutationTests(ApiTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_UpdatesProfile()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        Context.Profiles.Add(new Profile
        {
            UserId = UserId,
            Username = "old_username",
            DisplayName = "Old Display",
            Name = "Old Name",
            Surname = "Old Surname",
            AvatarUrl = "https://planara.local/old.png",
            Bio = "Old bio"
        });

        await Context.SaveChangesAsync();

        const string mutation = """
            mutation UpdateProfile($request: UpdateProfileRequestInput!) {
              updateProfile(request: $request) {
                username
                displayName
                name
                surname
                avatarUrl
                bio
              }
            }
            """;

        var variables = new
        {
            request = new
            {
                username = "new_username",
                displayName = "New Display",
                name = "New Name",
                surname = "New Surname",
                avatarUrl = "https://planara.local/new.png",
                bio = "New bio"
            }
        };

        using var json = await Client.PostAsync(mutation, variables);

        json.GetErrors().Should().BeNull();

        var profile = json.GetData().GetProperty("updateProfile");

        profile.GetProperty("username").GetString().Should().Be("new_username");
        profile.GetProperty("displayName").GetString().Should().Be("New Display");
        profile.GetProperty("name").GetString().Should().Be("New Name");
        profile.GetProperty("surname").GetString().Should().Be("New Surname");
        profile.GetProperty("avatarUrl").GetString().Should().Be("https://planara.local/new.png");
        profile.GetProperty("bio").GetString().Should().Be("New bio");

        Context.ChangeTracker.Clear();
        
        var saved = await Context.Profiles.SingleAsync(x => x.UserId == UserId);

        saved.Username.Should().Be("new_username");
        saved.DisplayName.Should().Be("New Display");
        saved.Name.Should().Be("New Name");
        saved.Surname.Should().Be("New Surname");
        saved.AvatarUrl.Should().Be("https://planara.local/new.png");
        saved.Bio.Should().Be("New bio");
    }

    [Fact]
    public async Task UpdateProfile_ProfileNotFound_ReturnsError()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        const string mutation = """
            mutation UpdateProfile($request: UpdateProfileRequestInput!) {
              updateProfile(request: $request) {
                username
                displayName
              }
            }
            """;

        var variables = new
        {
            request = new
            {
                username = "new_username"
            }
        };

        using var json = await Client.PostAsync(mutation, variables);

        json.GetErrors().Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateProfile_NoFieldsProvided_DoesNotChangeProfile()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        var profile = new Profile
        {
            UserId = UserId,
            Username = "old_username",
            DisplayName = "Old Display",
            Name = "Old Name",
            Surname = "Old Surname",
            AvatarUrl = "https://planara.local/old.png",
            Bio = "Old bio"
        };

        Context.Profiles.Add(profile);
        await Context.SaveChangesAsync();

        const string mutation = """
            mutation UpdateProfile($request: UpdateProfileRequestInput!) {
              updateProfile(request: $request) {
                username
                displayName
                name
                surname
                avatarUrl
                bio
              }
            }
            """;

        var variables = new
        {
            request = new { }
        };

        using var json = await Client.PostAsync(mutation, variables);

        json.GetErrors().Should().BeNull();

        var result = json.GetData().GetProperty("updateProfile");

        result.GetProperty("username").GetString().Should().Be("old_username");
        result.GetProperty("displayName").GetString().Should().Be("Old Display");
        result.GetProperty("name").GetString().Should().Be("Old Name");
        result.GetProperty("surname").GetString().Should().Be("Old Surname");
        result.GetProperty("avatarUrl").GetString().Should().Be("https://planara.local/old.png");
        result.GetProperty("bio").GetString().Should().Be("Old bio");
    }

    [Fact]
    public async Task UpdateProfile_NullFieldsProvided_DoesNotChangeProfile()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        Context.Profiles.Add(new Profile
        {
            UserId = UserId,
            Username = "old_username",
            DisplayName = "Old Display",
            Name = "Old Name",
            Surname = "Old Surname",
            AvatarUrl = "https://planara.local/old.png",
            Bio = "Old bio"
        });

        await Context.SaveChangesAsync();

        const string mutation = """
            mutation UpdateProfile($request: UpdateProfileRequestInput!) {
              updateProfile(request: $request) {
                username
                displayName
                name
                surname
                avatarUrl
                bio
              }
            }
            """;

        var variables = new
        {
            request = new
            {
                username = (string?)null,
                displayName = (string?)null,
                name = (string?)null,
                surname = (string?)null,
                avatarUrl = (string?)null,
                bio = (string?)null
            }
        };

        using var json = await Client.PostAsync(mutation, variables);

        json.GetErrors().Should().BeNull();

        var result = json.GetData().GetProperty("updateProfile");

        result.GetProperty("username").GetString().Should().Be("old_username");
        result.GetProperty("displayName").GetString().Should().Be("Old Display");
        result.GetProperty("name").GetString().Should().Be("Old Name");
        result.GetProperty("surname").GetString().Should().Be("Old Surname");
        result.GetProperty("avatarUrl").GetString().Should().Be("https://planara.local/old.png");
        result.GetProperty("bio").GetString().Should().Be("Old bio");
    }

    [Fact]
    public async Task UpdateProfile_SameValuesProvided_DoesNotChangeValues()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        Context.Profiles.Add(new Profile
        {
            UserId = UserId,
            Username = "same_username",
            DisplayName = "Same Display",
            Name = "Same Name",
            Surname = "Same Surname",
            AvatarUrl = "https://planara.local/same.png",
            Bio = "Same bio"
        });

        await Context.SaveChangesAsync();

        const string mutation = """
            mutation UpdateProfile($request: UpdateProfileRequestInput!) {
              updateProfile(request: $request) {
                username
                displayName
                name
                surname
                avatarUrl
                bio
              }
            }
            """;

        var variables = new
        {
            request = new
            {
                username = "same_username",
                displayName = "Same Display",
                name = "Same Name",
                surname = "Same Surname",
                avatarUrl = "https://planara.local/same.png",
                bio = "Same bio"
            }
        };

        using var json = await Client.PostAsync(mutation, variables);

        json.GetErrors().Should().BeNull();

        var result = json.GetData().GetProperty("updateProfile");

        result.GetProperty("username").GetString().Should().Be("same_username");
        result.GetProperty("displayName").GetString().Should().Be("Same Display");
        result.GetProperty("name").GetString().Should().Be("Same Name");
        result.GetProperty("surname").GetString().Should().Be("Same Surname");
        result.GetProperty("avatarUrl").GetString().Should().Be("https://planara.local/same.png");
        result.GetProperty("bio").GetString().Should().Be("Same bio");
    }
}