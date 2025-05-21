using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class UserController : Controller
{
    private readonly IConfiguration _Configuration;
    private HttpClient _client;

    public UserController(IConfiguration configuration)
    {
        _Configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_Configuration["WebApiBaseUrl"])
        };
    }
    
    public async Task<IActionResult> GetSellers()
    {
        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync("api/Users");

        var data = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var deserializedResponse = JsonConvert.DeserializeObject<IEnumerable<UserModel>>(data);
            return View("GetSellers",deserializedResponse);
        }

        return View("GetSellers",new List<UserModel>());
    }
    
    public async Task<IActionResult> AddEdit(int? id)
    {
        if (id == null)
        {
            return View(new UserModel());
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        HttpResponseMessage response = await _client.GetAsync($"api/Users/{id}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            return View(user);
        }

        return RedirectToAction("GetSellers");
    }

    [HttpPost]
    public async Task<IActionResult> Save(UserModel user)
    {
        if (!ModelState.IsValid)
        {
            return View("AddEdit", user);
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        // Ensure the client is initialized properly
        _client.DefaultRequestHeaders.Clear();

        // Add the Authorization header with the Bearer token (if available)
        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        if (user.UserId == 0) // Add new user
        {
            response = await _client.PostAsync("api/Users", jsonContent);
        }
        else // Update existing user
        {
            response = await _client.PutAsync($"api/Users/{user.UserId}", jsonContent);
        }

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetSellers");
        }

        ModelState.AddModelError(string.Empty, "Operation failed.");
        return View("AddEdit", user);
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
        
        HttpResponseMessage response = await _client.DeleteAsync($"api/Users/{id}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("GetSellers");
        }

        ModelState.AddModelError(string.Empty, "Failed to delete user.");
        return RedirectToAction("GetSellers");
    }
}
