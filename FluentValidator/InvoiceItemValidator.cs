using FluentValidation;
using Mobile_shop_Frontend.Models;

namespace Mobile_shop_Frontend.FluentValidator;

public class InvoiceItemModelValidator : AbstractValidator<InvoiceItemModel>
{
    public InvoiceItemModelValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice ID is required.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product name is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}