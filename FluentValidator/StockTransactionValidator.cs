using FluentValidation;
using Mobile_shop_Frontend.Models;

namespace Mobile_shop_Frontend.FluentValidator;

public class StockTransactionModelValidator : AbstractValidator<StockTransactionModel>
{
    public StockTransactionModelValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product name is required.");
        RuleFor(x => x.StockQuantity).GreaterThan(0).WithMessage("Stock Quantity must be greater than 0.");
        RuleFor(x => x.TransactionState).NotEmpty().WithMessage("Transaction State is required.");
        RuleFor(x => x.TransactionDescription).NotEmpty().WithMessage("Transaction Description is required.")
            .Length(2, 200).WithMessage("Description must be between 2 and 200 characters.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User name is required.");
    }
}