using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class InvoiceController : Controller
{
    private readonly IConfiguration _configuration;
    private HttpClient _client;

    public InvoiceController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }

    public async Task<IActionResult> GetInvoices(int? customerId = null, string status = null)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

        _client.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        // Build query parameters
        var queryParams = new List<string>();
        if (customerId.HasValue) queryParams.Add($"customerId={customerId}");
        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");

        string url = "api/Invoices";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }
        HttpResponseMessage response = await _client.GetAsync(url);
        var data = await response.Content.ReadAsStringAsync();

        IEnumerable<InvoiceModel> invoices = new List<InvoiceModel>();
        var customers = await FetchDropdownData("api/Dropdown/Customers");

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.Statuses = new SelectList(new[] { "Pending", "Paid" }, status);

        if (response.IsSuccessStatusCode)
        {
            invoices = JsonConvert.DeserializeObject<IEnumerable<InvoiceModel>>(data);
        }

        return View("GetInvoices", invoices);
    }
    public async Task<IActionResult> AddEdit(int? id)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        // Fetch customers and users for dropdowns
        var customers = await FetchDropdownData("api/Dropdown/Customers");
        var users = await FetchDropdownData("api/Dropdown/Users");

        ViewBag.Customers = new SelectList(customers, "Id", "Name");
        ViewBag.Users = new SelectList(users, "Id", "Name");

        if (id == null)
        {
            return View(new InvoiceModel());
        }

        var invoiceResponse = await _client.GetAsync($"api/Invoices/{id}");
        if (!invoiceResponse.IsSuccessStatusCode)
        {
            return RedirectToAction("GetInvoices");
        }

        var data = await invoiceResponse.Content.ReadAsStringAsync();
        var invoice = JsonConvert.DeserializeObject<InvoiceModel>(data);

        ViewBag.Customers = new SelectList(customers, "Id", "Name", invoice.CustomerId);
        ViewBag.Users = new SelectList(users, "Id", "Name", invoice.UserId);

        return View(invoice);
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
    public async Task<IActionResult> Save(InvoiceModel invoice)
    {
        if (!ModelState.IsValid)
        {
            var customers = await FetchDropdownData("api/Dropdown/Customers");
            var users = await FetchDropdownData("api/Dropdown/Users");

            ViewBag.Customers = new SelectList(customers, "Id", "Name", invoice.CustomerId);
            ViewBag.Users = new SelectList(users, "Id", "Name", invoice.UserId);

            return View("AddEdit", invoice);
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        if (invoice.InvoiceId == 0)
        {
            invoice.TotalAmount = 0;
        }
        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");
        HttpResponseMessage response = invoice.InvoiceId == 0
            ? await _client.PostAsync("api/Invoices", jsonContent)
            : await _client.PutAsync($"api/Invoices/{invoice.InvoiceId}", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Invoice saved successfully.";
            return RedirectToAction("GetInvoices");
        }

        var errorContent = await response.Content.ReadAsStringAsync();

        ModelState.AddModelError(string.Empty, $"Operation failed: {errorContent}");

        return View("AddEdit", invoice);
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
        
        HttpResponseMessage response = await _client.DeleteAsync($"api/Invoices/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetInvoices");
        }

        ModelState.AddModelError(string.Empty, "Failed to delete invoice.");
        return RedirectToAction("GetInvoices");
    }
}
