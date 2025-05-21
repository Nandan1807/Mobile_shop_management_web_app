using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class AdminDashboardController : Controller
{
    private readonly IConfiguration _configuration;
    private HttpClient _client;

    public AdminDashboardController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }
    public async Task<IActionResult> AdminDashboard(AdminDashboardModel model)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));
        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        
        var userJson = HttpContext.Session.GetString("UserDetails");
        var userDetails = JsonConvert.DeserializeObject<UserModel>(userJson);

        try
        {
            // Set default threshold if not provided
            model.Threshold ??= 30;

            // Fetch statistics
            HttpResponseMessage statsResponse = await _client.GetAsync("api/Dashboard/statistics");
            if (statsResponse.IsSuccessStatusCode)
            {
                model.Statistics = await statsResponse.Content.ReadFromJsonAsync<DashboardStatisticsModel>();
            }

            // Fetch low-stock products with threshold
            HttpResponseMessage lowStockResponse =
                await _client.GetAsync($"api/Dashboard/low-stock-products/{model.Threshold}");
            if (lowStockResponse.IsSuccessStatusCode)
            {
                model.LowStockProducts = await lowStockResponse.Content.ReadFromJsonAsync<List<LowStockProductModel>>() ?? new List<LowStockProductModel>();
            }
            
            // // Fetch customers for dropdown
            HttpResponseMessage customersResponse = await _client.GetAsync("api/Dropdown/Customers");
            if (customersResponse.IsSuccessStatusCode)
            {
                var customers = await customersResponse.Content.ReadFromJsonAsync<List<DropdownItemModel>>();
                
                if (customers != null && customers.Any())
                {
                    model.CustomerId ??= customers.First().Id;
                    ViewBag.Customers = new SelectList(customers, "Id", "Name", model.CustomerId);
                }
                else
                {
                    ViewBag.Customers = new SelectList(Enumerable.Empty<DropdownItemModel>(), "Id", "Name");
                }
            }
            //
            // Fetch purchase history with selected customer or default
            if (model.CustomerId.HasValue)
            {
                HttpResponseMessage historyResponse =
                    await _client.GetAsync($"api/Dashboard/customer-purchase-history/{model.CustomerId}");
                if (historyResponse.IsSuccessStatusCode)
                {
                    model.PurchaseHistory =
                        await historyResponse.Content.ReadFromJsonAsync<List<CustomerPurchaseHistoryModel>>() ?? new List<CustomerPurchaseHistoryModel>();
                }
            }
            
            // Set default dates for sales report
            model.EndDate ??= DateTime.Today;
            model.StartDate ??= model.EndDate.Value.AddDays(-30);
            
            // Fetch sales report
            HttpResponseMessage salesResponse = await _client.GetAsync(
                $"api/Dashboard/sales-report?startDate={model.StartDate:yyyy-MM-dd}&endDate={model.EndDate:yyyy-MM-dd}");
            if (salesResponse.IsSuccessStatusCode)
            {
                model.SalesReport = await salesResponse.Content.ReadFromJsonAsync<List<SalesReportModel>>() ?? new List<SalesReportModel>();
            }
            
            var trendsResponse = await _client.GetAsync($"api/SalesDashboard/sales-trend/{userDetails.UserId}");
            if (trendsResponse.IsSuccessStatusCode)
            {
                model.SalesTrends = await trendsResponse.Content.ReadFromJsonAsync<List<SalesTrendModel>>() ?? new();
            }
            
            // Fetch top-selling products
            var topProductsResponse = await _client.GetAsync($"api/SalesDashboard/top-selling-products/{userDetails.UserId}");
            if (topProductsResponse.IsSuccessStatusCode)
            {
                model.TopSellingProducts = await topProductsResponse.Content.ReadFromJsonAsync<List<TopSellingProductModel>>() ?? new();
            }

            // Fetch category-wise sales
            var categorySalesResponse = await _client.GetAsync($"api/SalesDashboard/sales-by-category/{userDetails.UserId}");
            if (categorySalesResponse.IsSuccessStatusCode)
            {
                model.CategorySales = await categorySalesResponse.Content.ReadFromJsonAsync<List<SalesByCategoryModel>>() ?? new();
            }

            // Fetch daily sales
            var dailySalesResponse = await _client.GetAsync($"api/SalesDashboard/daily-sales/{userDetails.UserId}");
            if (dailySalesResponse.IsSuccessStatusCode)
            {
                model.DailySales = await dailySalesResponse.Content.ReadFromJsonAsync<List<DailySalesModel>>() ?? new();
            }

            // Fetch top customers
            var topCustomersResponse = await _client.GetAsync($"api/SalesDashboard/top-customers/{userDetails.UserId}");
            if (topCustomersResponse.IsSuccessStatusCode)
            {
                model.TopCustomers = await topCustomersResponse.Content.ReadFromJsonAsync<List<TopCustomerModel>>() ?? new();
            }

            // Fetch payment status breakdown
            var paymentStatusResponse = await _client.GetAsync($"api/SalesDashboard/payment-status/{userDetails.UserId}");
            if (paymentStatusResponse.IsSuccessStatusCode)
            {
                model.PaymentStatusBreakdown = await paymentStatusResponse.Content.ReadFromJsonAsync<List<PaymentStatusModel>>() ?? new();
            }

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error loading dashboard data");
            return View(model);
        }
    }
}