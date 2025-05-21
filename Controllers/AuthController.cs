using System.Text;
using Microsoft.AspNetCore.Mvc;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

public class AuthController : Controller
{
    private readonly IConfiguration _Configuration;
    private HttpClient _client;

    public AuthController(IConfiguration configuration)
    {
        _Configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_Configuration["WebApiBaseUrl"])
        };
    }

    // public IActionResult SignUp()
    // {
    //     return View("SignUp");
    // }

    public IActionResult SignIn()
    {
        var userJson = HttpContext.Session.GetString("UserDetails");
    
        if (!string.IsNullOrEmpty(userJson))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return View("SignIn");
    }

    [HttpPost]
    public async Task<IActionResult> HandleSignIn(UserAuthModel userAuthModel)
    {
        if (ModelState.IsValid)
        {
            var json = JsonConvert.SerializeObject(userAuthModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("api/Users/signin", content);

            var data = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var deserializedResponse = JsonConvert.DeserializeObject<SignInResponse>(data);
                if (deserializedResponse != null)
                {
                    HttpContext.Session.SetString("UserDetails", JsonConvert.SerializeObject(deserializedResponse.UserDetails));
                    HttpContext.Session.SetString("AuthToken", JsonConvert.SerializeObject(deserializedResponse.AuthToken));
                    ViewBag.Message = deserializedResponse.Message;
                }
                return RedirectToAction("Index", "Dashboard");
            }

            var errorResponse = JsonConvert.DeserializeObject<SignInResponse>(data);
            ViewBag.Message = errorResponse?.Message;
            return View("SignIn");

        }
        return RedirectToAction("SignIn",userAuthModel);
    }
    //
    // public IActionResult HandleSignUp()
    // {
    //     return RedirectToAction("Index", "Dashboard");
    // }
    //
    [HttpPost]
    public async Task<IActionResult> HandleSignOut()
    {
        try
        {
            var userJson = HttpContext.Session.GetString("UserDetails");
            UserAuthModel userAuthModel = new UserAuthModel();

            if (!string.IsNullOrEmpty(userJson))
            {
                var userDetails = JsonConvert.DeserializeObject<UserModel>(userJson);
                userAuthModel = new UserAuthModel
                {
                    UserEmail = userDetails?.UserEmail,
                    Password = userDetails?.Password
                };
            }

            var json = JsonConvert.SerializeObject(userAuthModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("api/Users/signout", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Sign-out failed.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Remove session and redirect
            HttpContext.Session.Clear();
        
            return RedirectToAction("SignIn"); // Explicitly return a RedirectToActionResult
        }
        catch (Exception ex)
        {
            TempData["Message"] = "An error occurred during sign-out.";
            return RedirectToAction("Index", "Dashboard");
        }
    }


}