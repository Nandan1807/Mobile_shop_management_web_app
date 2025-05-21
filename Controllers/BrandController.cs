using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class BrandController : Controller
{
    private readonly IConfiguration _Configuration;
    private HttpClient _client;

    public BrandController(IConfiguration configuration)
    {
        _Configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_Configuration["WebApiBaseUrl"])
        };
    }
    
    public async Task<IActionResult> GetBrands()
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync("api/Brands");

        var data = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var deserializedResponse = JsonConvert.DeserializeObject<IEnumerable<BrandModel>>(data);
            return View("GetBrands", deserializedResponse);
        }

        return View("GetBrands",new List<BrandModel>());
    }
    
    public async Task<IActionResult> AddEdit(int? id)
    {
        if (id == null)
        {
            return View(new BrandModel());
        }
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync($"api/Brands/{id}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            var brand = JsonConvert.DeserializeObject<BrandModel>(data);
            return View(brand);
        }

        return RedirectToAction("GetBrands");
    }

    [HttpPost]
    public async Task<IActionResult> Save(BrandModel brand)
    {
        if (!ModelState.IsValid)
        {
            return View("AddEdit", brand);
        }

        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(brand), Encoding.UTF8, "application/json");

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response;
        if (brand.BrandId == 0) // Add new brand
        {
            response = await _client.PostAsync("api/Brands", jsonContent);
        }
        else // Update existing brand
        {
            response = await _client.PutAsync($"api/Brands/{brand.BrandId}", jsonContent);
        }

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetBrands");
        }

        ModelState.AddModelError(string.Empty, "Operation failed.");
        return View("AddEdit", brand);
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
        
        HttpResponseMessage response = await _client.DeleteAsync($"api/Brands/{id}");

        if (response.IsSuccessStatusCode) 
        {
            return RedirectToAction("GetBrands");
        }

        ModelState.AddModelError(string.Empty, "Failed to delete brand.");
        return RedirectToAction("GetBrands");
    }
}
