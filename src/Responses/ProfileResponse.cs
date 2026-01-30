using HotChocolate;

namespace Planara.Accounts.Responses;

/// <summary>
/// Профиль пользователя
/// </summary>
[GraphQLDescription("Профиль пользователя")]
public class ProfileResponse
{
    /// <summary>
    /// Никнейм пользователя
    /// </summary>
    [GraphQLDescription("Никнейм пользователя")]
    public string? Username { get; set; }
    
    /// <summary>
    /// Отображаемое имя пользователя
    /// </summary>
    [GraphQLDescription("Отображаемое имя пользователя")]
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [GraphQLDescription("Имя пользователя")]
    public string? Name { get; set; } = null!;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    [GraphQLDescription("Фамилия пользователя")]
    public string? Surname { get; set; } = null!;

    /// <summary>
    /// Ссылка на аватарку
    /// </summary>
    [GraphQLDescription("Ссылка на аватарку")]
    public string? AvatarUrl { get; set; } = null!;
    
    /// <summary>
    /// Описание профиля пользователя
    /// </summary>
    [GraphQLDescription("Описание профиля пользователя")]
    public string? Bio { get; set; }
}