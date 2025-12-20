using FluentValidation;
using WebAPIJwtAuth.Application.DTOs;

namespace WebAPIJwtAuth.Application.Validators
{
    public class CreateBrandValidator : AbstractValidator<CreateBrandDto>
    {
        public CreateBrandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Brand name is required")
                .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Website)
                .MaximumLength(200).WithMessage("Website must not exceed 200 characters")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage("Website must be a valid URL");
        }
    }
}