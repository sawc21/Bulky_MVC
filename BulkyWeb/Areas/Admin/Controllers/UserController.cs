using BulkyBook.DataAcess.Data;
using BulkyBook.DataAcess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(ApplicationDbContext db, IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {

            return View();
        }
        [HttpGet]
        public IActionResult RoleMangement(string id)
        {
            // load user with company via UnitOfWork
            var applicationUser = _unitOfWork.ApplicationUser
                .GetFirstOrDefault(u => u.Id == id, includeProperties: "Company");

            

            var applicationUserVM = new ApplicationUserVM
            {
                applicationUser = applicationUser,

                // list of all roles
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToList(),

                // list of all companies
                CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }).ToList()
            };

            // make the dropdown show the current role (blank if none)
            applicationUserVM.applicationUser.Role =_userManager.GetRolesAsync(_unitOfWork.ApplicationUser
                .GetFirstOrDefault(u => u.Id == id)).GetAwaiter().GetResult().FirstOrDefault();

            return View(applicationUserVM);
        }
        [HttpPost]
        public IActionResult RoleMangement(ApplicationUserVM applicationUserVM)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser
                .GetFirstOrDefault(u => u.Id == applicationUserVM.applicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == applicationUserVM.applicationUser.Id);

            if (applicationUserVM.applicationUser.Role != oldRole)
            {
                

                if (applicationUserVM.applicationUser.Role == SD.Role_Comp)
                {
                    applicationUser.CompanyId = applicationUserVM.applicationUser.CompanyId;
                }
                if(oldRole == SD.Role_Comp) 
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, applicationUserVM.applicationUser.Role).GetAwaiter().GetResult();
                


            }
            else
            {
                if(oldRole == SD.Role_Comp && applicationUser.CompanyId != applicationUserVM.applicationUser.CompanyId)
                {
                    applicationUser.CompanyId = applicationUserVM.applicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }                                              
            }

                // If the role hasn't changed, just redirect to Index (or you could return the view again if you want)
                return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

           
            foreach (var user in objUserList)
            {

                
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = objUserList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error whole Locking/Unlocking" });
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Completed" });
        }

        #endregion
    }
}
