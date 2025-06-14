using Backend.Api.Models;

namespace Backend.Configurations.Validators;
using FluentValidation;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Имя пользователя обязательно для заполнения")
            .MinimumLength(5)
            .WithMessage("Имя пользователя должно содержать минимум 5 символов")
            .Must(ContainsAtLeastOneLetter)
            .WithMessage("Имя пользователя должно содержать хотя бы одну букву");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен для заполнения")
            .MinimumLength(5)
            .WithMessage("Пароль должен содержать минимум 5 символов");
    }

    private static bool ContainsAtLeastOneLetter(string username)
    {
        return !string.IsNullOrWhiteSpace(username) && username.Any(char.IsLetter);
    }
}