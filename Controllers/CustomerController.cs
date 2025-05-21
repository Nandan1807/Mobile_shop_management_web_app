using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class CustomerController : Controller
{
    private readonly IConfiguration _Configuration;
    private HttpClient _client;

    public CustomerController(IConfiguration configuration)
    {
        _Configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_Configuration["WebApiBaseUrl"])
        };
    }
    
    public async Task<IActionResult> GetCustomers()
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync("api/Customers");
        var data = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var deserializedResponse = JsonConvert.DeserializeObject<IEnumerable<CustomerModel>>(data);
            return View("GetCustomers", deserializedResponse);
        }

        return View("GetCustomers",new List<CustomerModel>());
    }
    
    public async Task<IActionResult> AddEdit(int? id)
    {
        if (id == null)
        {
            return View(new CustomerModel());
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync($"api/Customers/{id}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<CustomerModel>(data);
            return View(customer);
        }

        return RedirectToAction("GetCustomers");
    }

    [HttpPost]
    public async Task<IActionResult> Save(CustomerModel customer)
    {
        if (!ModelState.IsValid)
        {
            return View("AddEdit", customer);
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        if (customer.CustomerId == 0) // Add new customer
        {
            response = await _client.PostAsync("api/Customers", jsonContent);
        }
        else // Update existing customer
        {
            response = await _client.PutAsync($"api/Customers/{customer.CustomerId}", jsonContent);
        }

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetCustomers");
        }

        ModelState.AddModelError(string.Empty, "Operation failed.");
        return View("AddEdit", customer);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.DeleteAsync($"api/Customers/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetCustomers");
        }

        ModelState.AddModelError(string.Empty, "Failed to delete customer.");
        return RedirectToAction("GetCustomers");
    }
}
