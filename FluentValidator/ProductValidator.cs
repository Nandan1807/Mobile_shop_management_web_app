using FluentValidation;
using Mobile_shop_Frontend.Models;

namespace Mobile_shop_Frontend.FluentValidator;

public class ProductModelValidator : AbstractValidator<ProductModel>
{
    public ProductModelValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters.");

        // ImageFile is required only when creating a new product (ProductId == 0)
        When(x => x.ProductId == 0, () =>
        {
            RuleFor(x => x.ImageFile)
                .NotNull().WithMessage("Product image is required.")
                .Must(BeAValidImage).WithMessage("Invalid image file. Only JPEG, PNG, and GIF are allowed.")
                .Must(BeAValidFileSize).WithMessage("Image file size must be less than 5MB.");
        });

        RuleFor(x => x.ProductPrice)
            .GreaterThan(0).WithMessage("Product price must be greater than 0.");

        RuleFor(x => x.ProductDescription)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category name is required.");

        RuleFor(x => x.ProductBrandId)
            .GreaterThan(0).WithMessage("Product Brand name is required.");
    }

    private bool BeAValidImage(IFormFile file)
    {
        if (file == null)
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        return allowedExtensions.Contains(fileExtension);
    }

    private bool BeAValidFileSize(IFormFile file)
    {
        if (file == null)
            return false;

        const int maxFileSize = 5 * 1024 * 1024; // 5MB
        return file.Length <= maxFileSize;
    }
}