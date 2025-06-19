using Backend.Api.Models;
using Backend.Domain.Enums;
using FluentValidation;

namespace Backend.Configurations.Validators;

public class CreateRoomValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.Genre)
            .NotEmpty()
            .WithMessage("Жанр обязателен для заполнения")
            .Must(BeValidGenre)
            .WithMessage("Указан недопустимый жанр");

        RuleFor(x => x.QuestionCount)
            .NotEmpty()
            .WithMessage("Количество вопросов обязательно для заполнения")
            .GreaterThan(0)
            .WithMessage("Количество вопросов должно быть больше 0");

        RuleFor(x => x.UserHostId)
            .NotEmpty()
            .WithMessage("ID создателя комнаты обязательно для заполнения");
    }

    private static bool BeValidGenre(string genre)
    {
        return Enum.TryParse<DeezerGenre>(genre, true, out _);
    }
}