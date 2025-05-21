using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

public class ProfileController : Controller
{
    private readonly IConfiguration _Configuration;
    private HttpClient _client;

    public ProfileController(IConfiguration configuration)
    {
        _Configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_Configuration["WebApiBaseUrl"])
        };
    }
    public IActionResult ProfilePage()
    {
        var userJson = HttpContext.Session.GetString("UserDetails");
        var userDetails = userJson != null ? JsonConvert.DeserializeObject<UserModel>(userJson) : new UserModel();
        return View(userDetails);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(UserModel user)
    {
        if (!ModelState.IsValid)
        {
            return View("ProfilePage", user);
        }

        var authToken = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("AuthToken"));


        _client.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(authToken))
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        }
        
        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        response = await _client.PutAsync($"api/Users/{user.UserId}", jsonContent);


        if (response.IsSuccessStatusCode)
        {
            HttpContext.Session.SetString("UserDetails",JsonConvert.SerializeObject(user));
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("ProfilePage");
        }
        
        TempData["ErrorMessage"] = "Operation failed. Please try again.";
        return View("ProfilePage", user);

    }
}