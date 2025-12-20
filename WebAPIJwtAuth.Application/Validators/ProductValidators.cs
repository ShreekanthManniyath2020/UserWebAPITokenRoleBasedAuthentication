using FluentValidation;
using WebAPIJwtAuth.Application.DTOs;

namespace WebAPIJwtAuth.Application.Validators
{
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
                .Matches(@"^[A-Z0-9-]+$").WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .ScalePrecision(2, 18).WithMessage("Price must have at most 2 decimal places");

            RuleFor(x => x.DiscountPrice)
                .GreaterThan(0).When(x => x.DiscountPrice.HasValue)
                .WithMessage("Discount price must be greater than 0")
                .LessThan(x => x.Price).When(x => x.DiscountPrice.HasValue)
                .WithMessage("Discount price must be less than regular price")
                .ScalePrecision(2, 18).When(x => x.DiscountPrice.HasValue)
                .WithMessage("Discount price must have at most 2 decimal places");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
        }
    }
}