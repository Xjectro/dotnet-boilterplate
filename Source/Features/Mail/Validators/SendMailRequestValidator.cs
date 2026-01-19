using FluentValidation;
using Source.Features.Mail.DTOs;

namespace Source.Features.Mail.Validators;

/// <summary>
/// FluentValidation validator for SendMailRequest
/// </summary>
public class SendMailRequestValidator : AbstractValidator<SendMailRequest>
{
    public SendMailRequestValidator()
    {
        RuleFor(x => x.To)
            .NotNull().WithMessage("Recipients are required")
            .NotEmpty().WithMessage("At least one recipient is required")
            .Must(emails => emails.All(email => IsValidEmail(email)))
            .WithMessage("All recipients must have valid email addresses");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MinimumLength(1).WithMessage("Subject cannot be empty")
            .MaximumLength(500).WithMessage("Subject cannot exceed 500 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required")
            .MinimumLength(1).WithMessage("Body cannot be empty")
            .MaximumLength(50000).WithMessage("Body cannot exceed 50000 characters");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
