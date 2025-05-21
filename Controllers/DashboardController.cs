using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mobile_shop_Frontend.Helper;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Controllers;

[AuthFilter]
public class DashboardController : Controller
{
    private readonly IConfiguration _configuration;
    private HttpClient _client;

    public DashboardController(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new HttpClient
        {
            BaseAddress = new Uri(_configuration["WebApiBaseUrl"])
        };
    }

    public Task<IActionResult> Index()
    {
        var userJson = HttpContext.Session.GetString("UserDetails");
        if (string.IsNullOrEmpty(userJson))
        {
            return Task.FromResult<IActionResult>(RedirectToAction("SignIn", "Auth"));
        }

        var userDetails = JsonConvert.DeserializeObject<UserModel>(userJson);
        ViewBag.UserDetails = userDetails;

        return Task.FromResult<IActionResult>(userDetails?.Role == "Admin" ? RedirectToAction("AdminDashboard","AdminDashboard") : RedirectToAction("SalesDashboard","SalesDashboard"));
    }

}