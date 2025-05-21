using FluentValidation;
using Mobile_shop_Frontend.Models;

namespace Mobile_shop_Frontend.FluentValidator;

public class BrandModelValidator : AbstractValidator<BrandModel>
{
    public BrandModelValidator()
    {
        RuleFor(x => x.BrandName)
            .NotEmpty().WithMessage("Brand name is required.")
            .Length(2, 100).WithMessage("Brand name must be between 2 and 100 characters.");
    }
}