using Planara.Common.Database.Domain;

namespace Planara.Accounts.Data.Domain;

/// <summary>
/// Профиль пользователя
/// </summary>
public class Profile: BaseEntity
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Никнейм пользователя
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Отображаемое имя пользователя
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? Name { get; set; } = null!;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string? Surname { get; set; } = null!;

    /// <summary>
    /// Ссылка на аватарку
    /// </summary>
    public string? AvatarUrl { get; set; } = null!;
    
    /// <summary>
    /// Описание профиля пользователя
    /// </summary>
    public string? Bio { get; set; }
}