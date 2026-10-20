using EnglishLearning.Application.DTOs;
using EnglishLearning.Domain.Enums;
using FluentValidation;

namespace EnglishLearning.Application.Validators;

public class StartConversationRequestValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationRequestValidator()
    {
        RuleFor(x => x.TopicId)
            .NotEmpty().WithMessage("Topic ID is required.");

        When(x => !string.IsNullOrWhiteSpace(x.EnglishLevel), () =>
        {
            RuleFor(x => x.EnglishLevel!)
                .IsEnumName(typeof(EnglishProficiencyLevel), caseSensitive: false)
                .WithMessage("Valid English proficiency level is required (A1, A2, B1, B2, C1, C2).");
        });
    }
}

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message text is required.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");
    }
}
