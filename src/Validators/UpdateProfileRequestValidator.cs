using FluentValidation;
using HotChocolate;
using Planara.Accounts.Requests;

namespace Planara.Accounts.Validators;

public class UpdateProfileRequestValidator: AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Username)
            .Must(BeNonEmptyWhenProvided)
            .WithMessage("Имя пользователя не может быть пустым.");

        RuleFor(x => x.DisplayName)
            .Must(BeNonEmptyWhenProvided)
            .WithMessage("Отображаемое имя не может быть пустым.");

        RuleFor(x => x)
            .Must(NotContainEmoji)
            .WithMessage("Использование эмодзи в полях профиля запрещено.");
    }
    
    private static bool BeNonEmptyWhenProvided(Optional<string?> opt)
    {
        if (!opt.HasValue) return true;
        
        if (opt.Value is null) return true;
        
        return !string.IsNullOrWhiteSpace(opt.Value);
    }
    
    private static bool NotContainEmoji(UpdateProfileRequest req)
    {
        return NoEmoji(req.Username)
               && NoEmoji(req.DisplayName)
               && NoEmoji(req.Name)
               && NoEmoji(req.Surname)
               && NoEmoji(req.AvatarUrl)
               && NoEmoji(req.Bio);
    }

    private static bool NoEmoji(Optional<string?> opt)
    {
        if (!opt.HasValue || opt.Value is null) return true;
        return !ContainsEmoji(opt.Value);
    }
    
    private static bool ContainsEmoji(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            
            if (char.IsSurrogate(ch))
                return true;
            
            if (ch == '\uFE0F' || ch == '\uFE0E')
                return true;
            
            if (ch >= '\u2600' && ch <= '\u27BF')
                return true;
        }

        return false;
    }
}