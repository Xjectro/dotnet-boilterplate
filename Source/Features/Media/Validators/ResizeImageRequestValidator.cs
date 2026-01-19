using FluentValidation;
using Source.Features.Media.DTOs;

namespace Source.Features.Media.Validators;

/// <summary>
/// FluentValidation validator for ResizeImageRequestDto
/// </summary>
public class ResizeImageRequestValidator : AbstractValidator<ResizeImageRequestDto>
{
    public ResizeImageRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Image ID is required");

        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Width cannot exceed 10000 pixels");

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Height cannot exceed 10000 pixels");

        RuleFor(x => x.OutputFolder)
            .MaximumLength(500).WithMessage("Output folder path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.OutputFolder));
    }
}
