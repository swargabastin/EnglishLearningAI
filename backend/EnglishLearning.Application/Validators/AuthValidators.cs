using EnglishLearning.Application.DTOs;
using EnglishLearning.Domain.Enums;
using FluentValidation;

namespace EnglishLearning.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your password.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.EnglishLevel)
            .NotEmpty().WithMessage("English level is required.")
            .IsEnumName(typeof(EnglishProficiencyLevel), caseSensitive: false)
            .WithMessage("Valid English proficiency level is required (A1, A2, B1, B2, C1, C2).");

        RuleFor(x => x.LearningGoal)
            .NotEmpty().WithMessage("Learning goal is required.")
            .IsEnumName(typeof(LearningGoal), caseSensitive: false)
            .WithMessage("Valid learning goal is required.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
