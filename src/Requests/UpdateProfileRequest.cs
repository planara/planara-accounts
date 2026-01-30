using HotChocolate;

namespace Planara.Accounts.Requests;

/// <summary>
/// Запрос на обновление профиля
/// </summary>
[GraphQLDescription("Запрос на обновление профиля")]
public class UpdateProfileRequest
{
    /// <summary>
    /// Никнейм пользователя
    /// </summary>
    [GraphQLDescription("Никнейм пользователя")]
    public Optional<string?> Username { get; set; }
    
    /// <summary>
    /// Отображаемое имя пользователя
    /// </summary>
    [GraphQLDescription("Отображаемое имя пользователя")]
    public Optional<string?> DisplayName { get; set; }
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [GraphQLDescription("Имя пользователя")]
    public Optional<string?> Name { get; set; }
    
    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    [GraphQLDescription("Фамилия пользователя")]
    public Optional<string?> Surname { get; set; }
    
    /// <summary>
    /// Ссылка на аватарку
    /// </summary>
    [GraphQLDescription("Ссылка на аватарку")]
    public Optional<string?> AvatarUrl { get; set; }
    
    /// <summary>
    /// Описание профиля пользователя
    /// </summary>
    [GraphQLDescription("Описание профиля пользователя")]
    public Optional<string?> Bio { get; set; }
}