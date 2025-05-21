using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mobile_shop_Frontend.Models;
using Newtonsoft.Json;

namespace Mobile_shop_Frontend.Helper;

public class AuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (string.IsNullOrEmpty(session.GetString("UserDetails")) ||
            string.IsNullOrEmpty(session.GetString("AuthToken")))
        {
            context.Result = new RedirectToActionResult("SignIn", "Auth", null);
            return;
        }

        var userDetailsJson = session.GetString("UserDetails");
        var userDetails = JsonConvert.DeserializeObject<UserModel>(userDetailsJson);

        if (userDetails == null || string.IsNullOrEmpty(userDetails.Role))
        {
            context.Result = new RedirectToActionResult("SignIn", "Auth", null);
            return;
        }

        var adminControllers = new List<string>
            { "Brand", "Category", "AdminDashboard", "Product", "StockTransaction", "User" };

        var currentController = context.RouteData.Values["controller"]?.ToString();

        if (userDetails.Role != "Admin" && adminControllers.Contains(currentController))
        {
            context.Result = new RedirectToActionResult("Unauthorized", "Error", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}