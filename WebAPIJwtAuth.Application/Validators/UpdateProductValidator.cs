using FluentValidation;
using WebAPIJwtAuth.Application.DTOs;

namespace WebAPIJwtAuth.Application.Validators
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            //Include(new CreateProductValidator());
        }
    }
}