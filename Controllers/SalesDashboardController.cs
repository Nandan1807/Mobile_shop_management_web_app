using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class SalesDashboardController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _client;

    public SalesDashboardController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }

    public async Task<IActionResult> SalesDashboard(SalesDashboardModel model)
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
            ModelState.AddModelError("", $"Error loading sales dashboard data: {ex.Message}");
            return View(model);
        }
    }
}
