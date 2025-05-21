using FluentValidation;
using Mobile_shop_Frontend.Models;

namespace Mobile_shop_Frontend.FluentValidator;

public class InvoiceModelValidator : AbstractValidator<InvoiceModel>
{
    public InvoiceModelValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer name is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User name is required.");
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("Payment Method is required.");
        RuleFor(x => x.PaymentStatus).NotEmpty().WithMessage("Payment Status is required.");
    }
}