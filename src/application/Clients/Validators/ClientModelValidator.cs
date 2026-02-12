using FluentValidation;
using Api.Core.Domain;

namespace Api.Application.Clients.Validators;

/// <summary>
/// FluentValidation validator for ClientModel
/// </summary>
public class ClientModelValidator : AbstractValidator<ClientModel>
{
    public ClientModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$").WithMessage("Name can only contain letters, numbers, spaces, hyphens, underscores, and dots");
    }
}
