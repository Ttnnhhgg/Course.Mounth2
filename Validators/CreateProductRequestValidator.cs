using FluentValidation;
using Shared.Models.DTOs;

namespace ProductManagement.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");
        }
    }

    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Name))
                .WithMessage("Product name must not exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).When(x => x.Price.HasValue)
                .WithMessage("Price must be greater than or equal to 0");
        }
    }
}