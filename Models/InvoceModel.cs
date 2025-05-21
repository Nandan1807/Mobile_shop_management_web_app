namespace Mobile_shop_Frontend.Models
{
    public class InvoiceModel
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? UserName { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
    }
}