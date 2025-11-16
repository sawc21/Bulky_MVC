using BulkyBook.DataAcess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Cast User to ClaimsPrincipal so FindFirst is available
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            // Get the logged-in user's ID
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            

            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
                    .GetAll(sc => sc.ApplicationUserId == claim.Value)
                    .Count());
                }
                
                   

                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }

               
        }
    }
}