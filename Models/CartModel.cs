namespace Mobile_shop_Frontend.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string? ProductImage { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }

    // Computed property (not stored) to calculate total price
    public decimal TotalPrice => ProductPrice * Quantity;
}

public class InvoiceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int InvoiceId { get; set; }
}

public class CheckoutRequest
{
    public int CustomerId { get; set; }
    public string PaymentMethod { get; set; }
}