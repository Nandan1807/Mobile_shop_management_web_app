using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class ProductController : Controller
{
    private readonly IConfiguration _configuration;
    private HttpClient _client;
    private readonly string _baseUrl;

    public ProductController(IConfiguration configuration)
    {
        _configuration = configuration;
        _baseUrl = _configuration["WebApiBaseUrl"];
        _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task<IActionResult> GetProducts(int? categoryId = null, int? brandId = null)
    {
        var authToken = HttpContext.Session.GetString("AuthToken") != null 
            ? JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken")) 
            : null;

        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        var queryParams = new List<string>();
        if (categoryId.HasValue) queryParams.Add($"categoryId={categoryId}");
        if (brandId.HasValue) queryParams.Add($"brandId={brandId}");

        string url = "api/Product";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        HttpResponseMessage response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return View("GetProducts", new List<ProductModel>());

        var data = await response.Content.ReadAsStringAsync();
        var products = JsonConvert.DeserializeObject<List<ProductModel>>(data) ?? new List<ProductModel>();

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

        return View("GetProducts", products);
    }

    public async Task<IActionResult> AddEdit(int? id)
    {
        var categories = await FetchDropdownData("api/Dropdown/Categories");
        var brands = await FetchDropdownData("api/Dropdown/Brands");

        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.Brands = new SelectList(brands, "Id", "Name");

        if (id == null) return View(new ProductModel());

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));
        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        var response = await _client.GetAsync($"api/Product/{id}");
        if (!response.IsSuccessStatusCode) return RedirectToAction("GetProducts");

        var data = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<ProductModel>(data);

        if (product != null && !string.IsNullOrEmpty(product.ProductImage))
        {
            product.ProductImage = $"{_baseUrl}{product.ProductImage}";
        }

        ViewBag.Categories = new SelectList(categories, "Id", "Name", product?.CategoryId);
        ViewBag.Brands = new SelectList(brands, "Id", "Name", product?.ProductBrandId);

        return View(product);
    }


    private async Task<List<DropdownItemModel>> FetchDropdownData(string endpoint)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        var response = await _client.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            return new List<DropdownItemModel>();
        }

        var data = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<DropdownItemModel>>(data);
    }

    [HttpPost]
    public async Task<IActionResult> Save(ProductModel product)
    {
        if (!ModelState.IsValid)
        {
            var categories = await FetchDropdownData("api/Dropdown/Categories");
            var brands = await FetchDropdownData("api/Dropdown/Brands");

            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(brands, "Id", "Name", product.ProductBrandId);

            return View("AddEdit", product);
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));
        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(product.ProductId.ToString()), "ProductId");
        form.Add(new StringContent(product.CategoryId.ToString()), "CategoryId");
        form.Add(new StringContent(product.ProductBrandId.ToString()), "ProductBrandId");
        form.Add(new StringContent(product.ProductName), "ProductName");
        form.Add(new StringContent(product.ProductPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)), "ProductPrice");
        form.Add(new StringContent(product.StockQuantity.ToString()), "StockQuantity");
        form.Add(new StringContent(product.Status), "Status");
        form.Add(new StringContent(product.ProductDescription ?? ""), "ProductDescription");

        if (product.ImageFile != null)
        {
            var streamContent = new StreamContent(product.ImageFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(product.ImageFile.ContentType);
            form.Add(streamContent, "ImageFile", product.ImageFile.FileName);
        }

        HttpResponseMessage response;
        if (product.ProductId == 0)
        {
            response = await _client.PostAsync("api/Product", form);
        }
        else
        {
            response = await _client.PutAsync($"api/Product/{product.ProductId}", form);
        }

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Product saved successfully.";
            return RedirectToAction("GetProducts");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Operation failed. Error: {errorContent}");
            return View("AddEdit", product);
        }

        ModelState.AddModelError(string.Empty, "Operation failed. Please try again.");
        return View("AddEdit", product);
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

    public async Task<IActionResult> Delete(int id)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        HttpResponseMessage response = await _client.DeleteAsync($"api/Product/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetProducts");
        }

        ModelState.AddModelError(string.Empty, "Failed to delete product.");
        return RedirectToAction("GetProducts");
    }

    private string GetCategoryNameById(int categoryId)
    {
        var categories = ViewBag.Categories as List<SelectListItem>;
        var category = categories?.FirstOrDefault(c => Convert.ToInt32(c.Value) == categoryId);
        return category?.Text ?? string.Empty;
    }

    private string GetBrandNameById(int productBrandId)
    {
        var brands = ViewBag.Brands as List<SelectListItem>;
        var brand = brands?.FirstOrDefault(b => Convert.ToInt32(b.Value) == productBrandId);
        return brand?.Text ?? string.Empty;
    }
    
}