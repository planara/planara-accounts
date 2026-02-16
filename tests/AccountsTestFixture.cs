namespace Planara.Accounts.Tests;

public class AccountsTestFixture: IAsyncLifetime
{
    protected readonly static ApiTestWebAppFactory Factory = new();

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}