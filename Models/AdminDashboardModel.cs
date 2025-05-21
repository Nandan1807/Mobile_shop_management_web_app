namespace Mobile_shop_Frontend.Models;

public class AdminDashboardModel
{
    public DashboardStatisticsModel Statistics { get; set; } = new();
    public List<LowStockProductModel> LowStockProducts { get; set; } = new();
    public List<CustomerPurchaseHistoryModel> PurchaseHistory { get; set; } = new();
    public List<SalesReportModel> SalesReport { get; set; } = new();
    public List<SalesTrendModel> SalesTrends { get; set; } = new();
    public List<TopSellingProductModel> TopSellingProducts { get; set; } = new();
    public List<SalesByCategoryModel> CategorySales { get; set; } = new();
    public List<DailySalesModel> DailySales { get; set; } = new();
    public List<TopCustomerModel> TopCustomers { get; set; } = new();
    public List<PaymentStatusModel> PaymentStatusBreakdown { get; set; } = new();
    public int? Threshold { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DashboardStatisticsModel
{
    public int TotalCustomers { get; set; }
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public decimal TotalSales { get; set; }
}

public class LowStockProductModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int StockQuantity { get; set; }
}

public class CustomerPurchaseHistoryModel
{
    public int InvoiceId { get; set; }
    public DateTime Date { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class SalesReportModel
{
    public int InvoiceId { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
}