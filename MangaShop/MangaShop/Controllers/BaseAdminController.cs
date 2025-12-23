using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MangaShop.Controllers
{
    public class BaseAdminController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var admin = context.HttpContext.Session.GetString("AdminLogin");

            if (string.IsNullOrEmpty(admin))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "NvbAdmin",
                    null
                );
            }

            base.OnActionExecuting(context);
        }
    }
}
