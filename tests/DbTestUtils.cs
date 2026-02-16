using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data;

namespace Planara.Accounts.Tests;

public class DbTestUtils
{
    public static async Task ResetAccountsDbAsync(DataContext db, CancellationToken cancellationToken = default)
    {
        await db.Database
            .ExecuteSqlRawAsync(@"TRUNCATE TABLE ""Profiles"" RESTART IDENTITY CASCADE;", cancellationToken);
    }
}