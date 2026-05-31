using FluentAssertions;
using HotChocolate;
using Planara.Accounts.Requests;
using Planara.Accounts.Validators;

namespace Planara.Accounts.Tests.Unit;

public class ValidatorsTests
{
    [Fact]
    public void UpdateProfile_UsernameNotProvided_Succeeds()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Username = default,
            DisplayName = default,
            Name = default,
            Surname = default,
            AvatarUrl = default,
            Bio = default
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_UsernameProvidedEmpty_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>(""),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("Имя пользователя не может быть пустым", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_UsernameProvidedWhitespace_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>("   "),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("Имя пользователя не может быть пустым", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_DisplayNameProvidedEmpty_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            DisplayName = new Optional<string?>(""),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("Отображаемое имя не может быть пустым", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_DisplayNameProvidedWhitespace_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            DisplayName = new Optional<string?>(" \t "),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("Отображаемое имя не может быть пустым", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_OptionalFieldProvidedNull_Succeeds()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>(null),
            DisplayName = new Optional<string?>(null),
            Bio = new Optional<string?>(null)
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_UsernameContainsEmoji_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>("user😀"),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("эмодзи", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_BioContainsEmoji_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Bio = new Optional<string?>("Привет 🌍"),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("эмодзи", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_AvatarUrlContainsEmoji_Fails()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            AvatarUrl = new Optional<string?>("https://planara/avatar😅.png"),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("эмодзи", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UpdateProfile_ValidFields_Succeeds()
    {
        var validator = new UpdateProfileRequestValidator();
        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>("planara_user"),
            DisplayName = new Optional<string?>("Planara User"),
            Name = new Optional<string?>("Ivan"),
            Surname = new Optional<string?>("Petrov"),
            AvatarUrl = new Optional<string?>("https://planara/avatar.png"),
            Bio = new Optional<string?>("Hello!")
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("user😀")]
    [InlineData("user🌍")]
    public void UpdateProfile_UsernameContainsDifferentEmojiRanges_Fails(string username)
    {
        var validator = new UpdateProfileRequestValidator();

        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>(username),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("эмодзи", StringComparison.OrdinalIgnoreCase));
    }
    
    [Theory]
    [InlineData("user\uFE0F")]
    [InlineData("user\uFE0E")]
    public void UpdateProfile_UsernameContainsVariationSelector_Fails(string username)
    {
        var validator = new UpdateProfileRequestValidator();

        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>(username),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("эмодзи", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("user\u2600")]
    [InlineData("user\u27BF")]
    public void UpdateProfile_UsernameContainsEmojiSymbolRange_Fails(string username)
    {
        var validator = new UpdateProfileRequestValidator();

        var request = new UpdateProfileRequest
        {
            Username = new Optional<string?>(username),
        };

        var res = validator.Validate(request);

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("эмодзи", StringComparison.OrdinalIgnoreCase));
    }
}