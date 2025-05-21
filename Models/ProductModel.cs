using System.ComponentModel.DataAnnotations.Schema;

namespace Mobile_shop_Frontend.Models;

public class ProductModel
{
    public int ProductId { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public string ProductName { get; set; }
    public string? ProductImage { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public decimal ProductPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ProductDescription { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int CategoryId { get; set; }
    public int ProductBrandId { get; set; }
}

