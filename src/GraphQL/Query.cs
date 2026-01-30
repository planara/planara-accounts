using System.Security.Claims;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data;
using Planara.Accounts.Responses;
using Planara.Common.Auth.Claims;
using Planara.Common.Exceptions;

namespace Planara.Accounts.GraphQL;

[ExtendObjectType(OperationTypeNames.Query)]
public class Query
{
    [Authorize]
    public async Task<ProfileResponse> GetProfile(
        [Service] DataContext dataContext,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var userId = claimsPrincipal.GetUserId();

        var profile = await dataContext.Profiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (profile is null)
            throw new NotFoundException();

        return new ProfileResponse
        {
            Name = profile?.Name,
            Surname = profile?.Surname,
            DisplayName = profile?.DisplayName ?? String.Empty,
            Username =  profile?.Username,
            Bio =  profile?.Bio,
        };
    }
}