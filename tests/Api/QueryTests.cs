using FluentAssertions;
using Planara.Accounts.Data.Domain;

namespace Planara.Accounts.Tests.Api;

public class QueryTests : BaseApiTest
{
    public QueryTests(ApiTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProfile_ExistingProfile_ReturnsCurrentUserProfile()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        Context.Profiles.Add(new Profile
        {
            UserId = UserId,
            Username = "planara_user",
            DisplayName = "Planara User",
            Name = "Ivan",
            Surname = "Petrov",
            AvatarUrl = "https://planara.local/avatar.png",
            Bio = "Hello!"
        });

        Context.Profiles.Add(new Profile
        {
            UserId = Guid.NewGuid(),
            Username = "foreign_user",
            DisplayName = "Foreign User",
            Name = "Petr",
            Surname = "Ivanov",
            Bio = "Foreign bio"
        });

        await Context.SaveChangesAsync();

        const string query = """
            query Profile {
              profile {
                username
                displayName
                name
                surname
                bio
              }
            }
            """;

        using var json = await Client.PostAsync(query);

        json.GetErrors().Should().BeNull();

        var profile = json.GetData().GetProperty("profile");

        profile.GetProperty("username").GetString().Should().Be("planara_user");
        profile.GetProperty("displayName").GetString().Should().Be("Planara User");
        profile.GetProperty("name").GetString().Should().Be("Ivan");
        profile.GetProperty("surname").GetString().Should().Be("Petrov");
        profile.GetProperty("bio").GetString().Should().Be("Hello!");
    }

    [Fact]
    public async Task GetProfile_ProfileNotFound_ReturnsError()
    {
        await DbTestUtils.ResetAccountsDbAsync(Context);

        const string query = """
            query Profile {
              profile {
                username
                displayName
              }
            }
            """;

        using var json = await Client.PostAsync(query);

        json.GetErrors().Should().NotBeNull();
    }
}