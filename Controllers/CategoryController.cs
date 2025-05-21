using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class CategoryController : Controller
{
    private readonly IConfiguration _configuration;
    private HttpClient _client;

    public CategoryController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }

    public async Task<IActionResult> GetCategories()
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync("api/Categories");
        var data = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var categories = JsonConvert.DeserializeObject<IEnumerable<CategoryModel>>(data);
            return View("GetCategories", categories);
        }
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}, {data}");
        }
        return View("GetCategories",new List<CategoryModel>());
    }

    public async Task<IActionResult> AddEdit(int? id)
    {
        if (id == null)
        {
            return View(new CategoryModel());
        }
        
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        HttpResponseMessage response = await _client.GetAsync($"api/Categories/{id}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<CategoryModel>(data);
            return View(category);
        }

        return RedirectToAction("GetCategories");
    }

    [HttpPost]
    public async Task<IActionResult> Save(CategoryModel category)
    {
        if (!ModelState.IsValid)
        {
            return View("AddEdit", category);
        }

        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
        
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response;

        if (category.CategoryId == 0) 
        {
            response = await _client.PostAsync("api/Categories", jsonContent);
        }
        else 
        {
            response = await _client.PutAsync($"api/Categories/{category.CategoryId}", jsonContent);
        }

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetCategories");
        }

        ModelState.AddModelError(string.Empty, "Operation failed.");
        return View("AddEdit", category);
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
        
        HttpResponseMessage response = await _client.DeleteAsync($"api/Categories/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetCategories");
        }

        ModelState.AddModelError(string.Empty, "Failed to delete category.");
        return RedirectToAction("GetCategories");
    }
}
