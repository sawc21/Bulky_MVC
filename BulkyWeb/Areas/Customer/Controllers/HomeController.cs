using BulkyBook.DataAcess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()

        {

            
            IEnumerable<Product> ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(ProductList);
        }

        public IActionResult Details(int id)
        {
            
            

            var cart = new ShoppingCart
            {
                ProductId = id,

                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,ProductImages"), // for display only; don't post it back
                Count = 1
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Details([Bind("ProductId,Count")] ShoppingCart shoppingCart)
        {
            // Get user id safely
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Never trust posted Id/Product
            shoppingCart.ApplicationUserId = userId;

            // Upsert: if same product already in this user's cart, just bump Count
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                sc => sc.ApplicationUserId == userId && sc.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                TempData["success"] = "Item added to cart successfully";
                _unitOfWork.Save();
            }
            else
            {
                // Ensure EF doesn't try to insert Product from the nav prop
                // (we didn't bind Product, so it's null; that's good)
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(
                sc => sc.ApplicationUserId == userId).Count());
                TempData["success"] = "Item added to cart successfully";
            }
            

            
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
