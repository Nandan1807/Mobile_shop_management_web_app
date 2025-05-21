using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;


namespace Mobile_shop_Frontend.Controllers
{
    [AuthFilter]
    public class InvoiceItemController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public InvoiceItemController(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new HttpClient
            {
                BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceItems(int invoiceId)
        {
            var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


            // Ensure the client is initialized properly
            _client.DefaultRequestHeaders.Clear();

            // Add the Authorization header with the Bearer token (if available)
            if (!string.IsNullOrEmpty(authToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            }
            
            HttpResponseMessage response = await _client.GetAsync($"api/InvoiceItems/invoice/{invoiceId}");
            var data = await response.Content.ReadAsStringAsync();
            IEnumerable<InvoiceItemModel> invoiceItems = new List<InvoiceItemModel>();

            if (response.IsSuccessStatusCode)
            {
                invoiceItems = JsonConvert.DeserializeObject<IEnumerable<InvoiceItemModel>>(data);
            }

            ViewBag.InvoiceId = invoiceId;
            return View("GetInvoiceItems", invoiceItems);
        }


        public async Task<IActionResult> AddEdit(int? id, int invoiceId)
        {
            var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


            // Ensure the client is initialized properly
            _client.DefaultRequestHeaders.Clear();

            // Add the Authorization header with the Bearer token (if available)
            if (!string.IsNullOrEmpty(authToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            }
            
            var products = await FetchDropdownData("api/Dropdown/Products");
            ViewBag.Products = new SelectList(products, "Id", "Name");

            if (id == null)
            {
                return View(new InvoiceItemModel { InvoiceId = invoiceId });
            }

            var response = await _client.GetAsync($"api/InvoiceItems/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetInvoiceItems", new { invoiceId });
            }

            var data = await response.Content.ReadAsStringAsync();
            var invoiceItem = JsonConvert.DeserializeObject<InvoiceItemModel>(data);

            ViewBag.Products = new SelectList(products, "Id", "Name", invoiceItem.ProductId);
            return View(invoiceItem);
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
        public async Task<IActionResult> Save(InvoiceItemModel invoiceItem)
        {
            if (!ModelState.IsValid)
            {
                var products = await FetchDropdownData("api/Dropdown/Products");
                ViewBag.Products = new SelectList(products, "Id", "Name", invoiceItem.ProductId);
                return View("AddEdit", invoiceItem);
            }
            
            
            StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(invoiceItem), Encoding.UTF8, "application/json");
            var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


            // Ensure the client is initialized properly
            _client.DefaultRequestHeaders.Clear();

            // Add the Authorization header with the Bearer token (if available)
            if (!string.IsNullOrEmpty(authToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            }
            HttpResponseMessage response = invoiceItem.InvoiceItemId == 0
                ? await _client.PostAsync("api/InvoiceItems", jsonContent)
                : await _client.PutAsync($"api/InvoiceItems/{invoiceItem.InvoiceItemId}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Invoice item saved successfully.";
                return RedirectToAction("GetInvoiceItems", new { invoiceId = invoiceItem.InvoiceId });
            }

            var errorContent = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(string.Empty, $"Operation failed: {errorContent}");
            return View("AddEdit", invoiceItem);
        }

        public async Task<IActionResult> Delete(int id, int invoiceId)
        {
            var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


            // Ensure the client is initialized properly
            _client.DefaultRequestHeaders.Clear();

            // Add the Authorization header with the Bearer token (if available)
            if (!string.IsNullOrEmpty(authToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            }
            HttpResponseMessage response = await _client.DeleteAsync($"api/InvoiceItems/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("GetInvoiceItems", new { invoiceId });
            }

            ModelState.AddModelError(string.Empty, "Failed to delete invoice item.");
            return RedirectToAction("GetInvoiceItems", new { invoiceId });
        }
    }
}
