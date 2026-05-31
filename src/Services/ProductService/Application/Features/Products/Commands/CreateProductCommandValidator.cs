using System.Data;
using FluentValidation;

namespace ProductService.Application.Features.Products.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CreateProductRequest.Name)
                    .NotEmpty()
                    .MaximumLength(100);

        RuleFor( x => x.CreateProductRequest.Price)
                    .GreaterThanOrEqualTo(0);
    }
}