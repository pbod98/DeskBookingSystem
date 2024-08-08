using DeskBookingSystem.Helpers;
using DeskBookingSystem.Models;
using DeskBookingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeskBookingSystem.Controllers
{
    public class Account : Controller
    {
        private readonly ApplicationDbContext _context;
        public Account(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var existingUser = _context.Users.Where(u => u.UserName == viewModel.UserName).FirstOrDefault();
            var passwordHelper = new PasswordHelper();

            if(existingUser == null)
            {              
                var user = new User
                {
                    UserName = viewModel.UserName,
                    HashedPassword = passwordHelper.HashPassword(viewModel.Password)
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                var role = _context.Roles.Where(r => r.Name == "User").FirstOrDefault();
                var addedUser = _context.Users.Include(u => u.UserRoles).Where(u => u.Id == user.Id).FirstOrDefault();
                addedUser.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
                
                _context.SaveChanges(); 
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User with this name already exists.");
                return View(viewModel);
            }

            return RedirectToAction("SignIn");

        }
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignIn(UserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var passwordHelper = new PasswordHelper();
            var existingUser = _context.Users.Where(u => u.UserName == viewModel.UserName && u.HashedPassword == passwordHelper.HashPassword(viewModel.Password)).FirstOrDefault();

            if (existingUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard", new { userId = existingUser.Id });
            }
        }
    }
}
