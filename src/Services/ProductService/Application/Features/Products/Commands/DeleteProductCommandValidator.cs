using FluentValidation;
using ProductService.Application.Features.Products.Commands;

namespace ProductService.Application.Features.Products;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
                .NotNull()
                .NotEmpty()
                .WithMessage("Id Can not be null or empty...");
    }
}