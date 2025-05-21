using FluentValidation;
using Mobile_shop_Frontend.Models;

namespace Mobile_shop_Frontend.FluentValidator;

public class CustomerModelValidator : AbstractValidator<CustomerModel>
{
    public CustomerModelValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer Name is required.")
            .Length(2, 100).WithMessage("Customer Name must be between 2 and 100 characters.");
        RuleFor(x => x.CustomerEmail).NotEmpty().WithMessage("Customer Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.CustomerPhone).NotEmpty().WithMessage("Customer Phone is required.")
            .Length(10).WithMessage("Customer Phone must be 10 digits.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Customer Address is required.")
            .Length(5, 200).WithMessage("Customer Address must be between 5 and 200 characters.");
    }
}