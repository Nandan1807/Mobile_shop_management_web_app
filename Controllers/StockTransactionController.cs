using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class StockTransactionController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _client;

    public StockTransactionController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }

    public async Task<IActionResult> GetStockTransactions()
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        HttpResponseMessage response = await _client.GetAsync("api/StockTransactions");
        var data = await response.Content.ReadAsStringAsync();
        IEnumerable<StockTransactionModel> transactions = new List<StockTransactionModel>();

        if (response.IsSuccessStatusCode)
        {
            transactions = JsonConvert.DeserializeObject<IEnumerable<StockTransactionModel>>(data);
        }

        return View("GetStockTransactions", transactions);
    }

    public async Task<IActionResult> AddEdit(int? id)
    {
        var products = await FetchDropdownData("api/Dropdown/Products");
        var users = await FetchDropdownData("api/Dropdown/Users");

        ViewBag.Products = new SelectList(products, "Id", "Name");
        ViewBag.Users = new SelectList(users, "Id", "Name");

        return View(new StockTransactionModel());
        // if (id == null)
        // {
        //     return View(new StockTransactionModel());
        // }

        // var authToken = HttpContext.Session.GetString("AuthToken");
        //
        // // Ensure the client is initialized properly
        // _client.DefaultRequestHeaders.Clear();
        //
        // // Add the Authorization header with the Bearer token (if available)
        // if (!string.IsNullOrEmpty(authToken))
        // {
        //     _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        // }
        // var transactionResponse = await _client.GetAsync($"api/StockTransactions/{id}");
        // if (!transactionResponse.IsSuccessStatusCode)
        // {
        //     return RedirectToAction("GetStockTransactions");
        // }
        //
        // var data = await transactionResponse.Content.ReadAsStringAsync();
        // var transaction = JsonConvert.DeserializeObject<StockTransactionModel>(data);
        //
        // ViewBag.Products = new SelectList(products, "Id", "Name", transaction.ProductId);
        // ViewBag.Users = new SelectList(users, "Id", "Name", transaction.UserId);
        //
        // return View(transaction);
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
    public async Task<IActionResult> Save(StockTransactionModel transaction)
    {
        if (!ModelState.IsValid)
        {
            var products = await FetchDropdownData("api/Dropdown/Products");
            var users = await FetchDropdownData("api/Dropdown/Users");
            ViewBag.Products = new SelectList(products, "Id", "Name", transaction.ProductId);
            ViewBag.Users = new SelectList(users, "Id", "Name", transaction.UserId);
            return View("AddEdit", transaction);
        }
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
        HttpResponseMessage response = transaction.TransactionId == 0
            ? await _client.PostAsync("api/StockTransactions", jsonContent)
            : await _client.PutAsync($"api/StockTransactions/{transaction.TransactionId}", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Transaction saved successfully.";
            return RedirectToAction("GetStockTransactions");
        }

        // Extract error message from response
        var errorContent = await response.Content.ReadAsStringAsync();

        ModelState.AddModelError(string.Empty, $"Operation failed: {errorContent}");

        return View("AddEdit", transaction);
    }

    

    // public async Task<IActionResult> Delete(int id)
    // {
    //     HttpResponseMessage response = await _client.DeleteAsync($"api/StockTransactions/{id}");
    //
    //     if (response.IsSuccessStatusCode)
    //     {
    //         return RedirectToAction("GetStockTransactions");
    //     }
    //
    //     ModelState.AddModelError(string.Empty, "Failed to delete transaction.");
    //     return RedirectToAction("GetStockTransactions");
    // }
}
