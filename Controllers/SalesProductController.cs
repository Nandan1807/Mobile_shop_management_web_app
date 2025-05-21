using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class SalesProductController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public SalesProductController(IConfiguration configuration)
    {
        _configuration = configuration;
        _baseUrl = _configuration["WebApiBaseUrl"];
        _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<IActionResult> GetProducts(int? categoryId = null, int? brandId = null)
    {
        try
        {
            SetAuthorizationHeader();

            var queryParams = new List<string>();
            if (categoryId.HasValue) queryParams.Add($"categoryId={categoryId}");
            if (brandId.HasValue) queryParams.Add($"brandId={brandId}");

            string url = "api/Product" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");

            HttpResponseMessage response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return View("GetSalesProducts", new List<ProductModel>());

            var data = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ProductModel>>(data) ?? new List<ProductModel>();

            // Append base URL to product images
            products.ForEach(product =>
            {
                if (!string.IsNullOrEmpty(product.ProductImage))
                {
                    product.ProductImage = $"{_baseUrl}{product.ProductImage}";
                }
            });

            var categories = await FetchDropdownData("api/Dropdown/Categories");
            var brands = await FetchDropdownData("api/Dropdown/Brands");

            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.Brands = new SelectList(brands, "Id", "Name", brandId);

            return View("GetSalesProducts", products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    private async Task<List<DropdownItemModel>> FetchDropdownData(string endpoint)
    {
        try
        {
            SetAuthorizationHeader();

            HttpResponseMessage response = await _client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return new List<DropdownItemModel>();
            }

            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DropdownItemModel>>(data);
        }
        catch
        {
            return new List<DropdownItemModel>();
        }
    }
    public async Task<IActionResult> GetProductDetails(int id)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));
        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        var response = await _client.GetAsync($"api/Product/{id}");
        if (!response.IsSuccessStatusCode) return Json(new { success = false });

        var data = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<ProductModel>(data);

        if (product != null && !string.IsNullOrEmpty(product.ProductImage))
        {
            product.ProductImage = $"{_baseUrl}{product.ProductImage}";
        }

        return Json(new
        {
            success = true,
            productImage = product?.ProductImage,
            productName = product?.ProductName,
            brandName = product?.BrandName,
            categoryName = product?.CategoryName,
            productPrice = product?.ProductPrice,
            stockQuantity = product?.StockQuantity,
            status = product?.Status,
            productDescription = product?.ProductDescription,
            createdDate = product?.CreatedDate.ToString("dd/MM/yyyy"),
            modifiedDate = product?.ModifiedDate?.ToString("dd/MM/yyyy")
        });
    }

    private void SetAuthorizationHeader()
    {
        var authToken = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JsonConvert.DeserializeObject<string>(authToken)}");
        }
    }
}
