using FluentValidation;
using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Validators.Products;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Price)
            .GreaterThan(0);
        
    }
}