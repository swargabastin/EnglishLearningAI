using EnglishLearning.Application.DTOs;
using FluentValidation;

namespace EnglishLearning.Application.Validators;

public class TopicDtoValidator : AbstractValidator<TopicDto>
{
    public TopicDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Topic name is required.")
            .MaximumLength(100).WithMessage("Topic name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Topic description is required.")
            .MaximumLength(1000).WithMessage("Topic description must not exceed 1000 characters.");

        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty level is required.");
    }
}
