using System.Security.Claims;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data;
using Planara.Accounts.Data.Domain;
using Planara.Common.Auth.Claims;

namespace Planara.Accounts.GraphQL;

[ExtendObjectType(OperationTypeNames.Query)]
public class Query
{
    [Authorize]
    public async Task<Profile?> GetProfile(
        [Service] DataContext dataContext,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        var profile = await dataContext.Profiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return profile;
    }
}