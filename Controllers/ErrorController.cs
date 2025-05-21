using Microsoft.AspNetCore.Mvc;

namespace Mobile_shop_Frontend.Controllers;

public class ErrorController : Controller
{
    public IActionResult Unauthorized()
    {
        return View();
    }

    public IActionResult Error404()
    {
        return View();
    }
}