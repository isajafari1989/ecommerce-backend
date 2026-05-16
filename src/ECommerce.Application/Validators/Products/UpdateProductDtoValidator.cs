using FluentValidation;
using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Validators.Products;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.Price)
            .GreaterThan(0);
        
    }
}