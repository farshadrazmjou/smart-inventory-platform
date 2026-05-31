using FluentValidation;

namespace ProductService.Application.Features.Products;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty();

        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.Stock).GreaterThanOrEqualTo(0);
    }
}