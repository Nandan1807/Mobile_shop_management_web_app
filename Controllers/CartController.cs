using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class CartController : Controller
{
    private readonly IConfiguration _configuration;
    private HttpClient _client;

    public CartController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }

    public async Task<IActionResult> Index(int? customerId)
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));
        _client.DefaultRequestHeaders.Clear();
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }

        HttpResponseMessage customersResponse = await _client.GetAsync("api/Dropdown/Customers");
        if (customersResponse.IsSuccessStatusCode)
        {
            var customers = await customersResponse.Content.ReadFromJsonAsync<List<DropdownItemModel>>();

            if (customers != null && customers.Any())
            {
                customerId ??= customers.First().Id;
                ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
            }
            else
            {
                ViewBag.Customers = new SelectList(Enumerable.Empty<DropdownItemModel>(), "Id", "Name");
            }
        }

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View("Cart", cart);
    }

    [HttpPost]
    public IActionResult AddToCart(int productId, string productName, string productImage, int productPrice,
        int quantity)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = productId,
                ProductName = productName,
                ProductImage = productImage,
                ProductPrice = productPrice,
                Quantity = quantity
            });
        }

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int productId, string action)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        if (item != null)
        {
            if (action == "increase") item.Quantity++;
            if (action == "decrease" && item.Quantity > 1) item.Quantity--;
        }

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        cart.RemoveAll(c => c.ProductId == productId);

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> HandleCheckout([FromBody] CheckoutRequest request)
    {
        if (request?.CustomerId == null)
        {
            return Json(new { success = false, message = "Invalid customer selection." });
        }

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        if (!cart.Any())
        {
            return Json(new { success = false, message = "Cart is empty." });
        }

        try
        {
            
            var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));

            // Ensure the client is initialized properly
            _client.DefaultRequestHeaders.Clear();

            // Add the Authorization header if available
            if (!string.IsNullOrEmpty(authToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            }
            else
            {
                return Json(new { success = false, message = "Authorization required. Please log in again." });
            }

            var userJson = HttpContext.Session.GetString("UserDetails");
            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var userDetails = JsonConvert.DeserializeObject<UserModel>(userJson);
            decimal totalAmount = cart.Sum(item => item.ProductPrice * item.Quantity);

            // Create Invoice Request
            var invoiceRequest = new
            {
                CustomerId = request.CustomerId,
                UserId = userDetails.UserId,
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending"
            };


            StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(invoiceRequest), Encoding.UTF8,
                "application/json");
            HttpResponseMessage invoiceResponse = await _client.PostAsync("api/Invoices", jsonContent);

            if (!invoiceResponse.IsSuccessStatusCode)
            {
                string errorResponse = await invoiceResponse.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Failed to create invoice: {errorResponse}" });
            }

            var invoiceResponseContent = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>();

            if (invoiceResponseContent == null || !invoiceResponseContent.Success ||
                invoiceResponseContent.InvoiceId == 0)
            {
                return Json(new { success = false, message = "Invalid response from invoice creation." });
            }

            int invoiceId = invoiceResponseContent.InvoiceId;

            // Add Invoice Items
            foreach (var item in cart)
            {
                var invoiceItemRequest = new
                {
                    InvoiceId = invoiceId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };


                StringContent invoiceItemJson = new StringContent(JsonConvert.SerializeObject(invoiceItemRequest),
                    Encoding.UTF8, "application/json");
                HttpResponseMessage invoiceItemResponse = await _client.PostAsync("api/InvoiceItems", invoiceItemJson);

                if (!invoiceItemResponse.IsSuccessStatusCode)
                {
                    string invoiceItemError = await invoiceItemResponse.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Failed to add {item.ProductName} to invoice." });
                }
            }

            // Clear cart after successful checkout
            HttpContext.Session.Remove("Cart");

            return Json(new { success = true, message = "Checkout successful, invoice created.", invoiceId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
        }
    }
}