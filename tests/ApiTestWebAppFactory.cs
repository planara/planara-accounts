using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Planara.Accounts.Data;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Planara.Accounts.Tests;

public class ApiTestWebAppFactory: WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("accounts-test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    private readonly RedisContainer _redis = new RedisBuilder("redis:latest").Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<DataContext>));
            services.RemoveAll(typeof(DataContext));
            services.RemoveAll(typeof(IConnectionMultiplexer));

            services.AddDbContext<DataContext>(opt =>
                opt.UseNpgsql(_postgres.GetConnectionString()));
        });
        
        builder.ConfigureAppConfiguration((config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>(
                    "DbConnections:Redis:ConnectionString", 
                    _redis.GetConnectionString()!),
                new KeyValuePair<string, string>(
                    "GraphQL:Name", 
                    "test-auth-schema")
            }!);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();

        db.Database.SetCommandTimeout(3000);
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await _redis.StopAsync();
    }
}