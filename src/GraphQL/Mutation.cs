using System.Security.Claims;
using AppAny.HotChocolate.FluentValidation;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data;
using Planara.Accounts.Requests;
using Planara.Accounts.Responses;
using Planara.Accounts.Validators;
using Planara.Common.Auth.Claims;
using Planara.Common.Exceptions;

namespace Planara.Accounts.GraphQL;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class Mutation
{
    /// <summary>
    /// Обновление профиля пользователя
    /// </summary>
    /// <param name="dataContext">Контекст базы данных</param>
    /// <param name="claimsPrincipal">Клеймы пользователя, для получения ID</param>
    /// <param name="request">Запрос на обновление профиля пользователя</param>
    /// <param name="cancellationToken">Токен отмены асинхронной операции</param>
    /// <exception cref="NotFoundException">Исключение, если профиль не был найден</exception>
    [Authorize]
    [GraphQLDescription("Обновление профиля пользователя")]
    public async Task<ProfileResponse> UpdateProfile(
        [Service] DataContext dataContext,
        ClaimsPrincipal claimsPrincipal,
        [GraphQLDescription("Данные для обновления профиля")]
        [UseFluentValidation, UseValidator<UpdateProfileRequestValidator>]
        UpdateProfileRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();

        var profile = await dataContext.Profiles
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
            throw new NotFoundException();
        
        // Обновление профиля пользователя, только если пришли данные
        Patch(request.Username, profile.Username, v => profile.Username = v);
        Patch(request.DisplayName, profile.DisplayName, v => profile.DisplayName = v);
        Patch(request.Name, profile.Name, v => profile.Name = v);
        Patch(request.Surname, profile.Surname,v => profile.Surname = v);
        Patch(request.AvatarUrl, profile.AvatarUrl,v => profile.AvatarUrl = v);
        Patch(request.Bio, profile.Bio,v => profile.Bio = v);
        
        profile.UpdatedAt = DateTime.UtcNow;
        
        await dataContext.SaveChangesAsync(cancellationToken);
        
        return new ProfileResponse
        {
            Username = profile.Username,
            DisplayName = profile.DisplayName,
            Name = profile.Name,
            Surname = profile.Surname,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio
        };
    }
    
    /// <summary>
    /// Обновление полей сущности только при необходмости (передали новое значение)
    /// </summary>
    private static void Patch(Optional<string?> opt, string? oldValue, Action<string> set)
    {
        if (opt.HasValue && opt.Value is not null && !String.Equals(opt.Value, oldValue, StringComparison.Ordinal))
            set(opt.Value);
    }
}