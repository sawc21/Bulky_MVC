using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    public class CartControllre : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
