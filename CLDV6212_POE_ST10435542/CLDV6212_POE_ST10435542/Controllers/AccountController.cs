using CLDV6212_POE_ST10435542.Models;
using CLDV6212_POE_ST10435542.Models.Data;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CLDV6212_POE_ST10435542.Controllers
{
    // The LoginController is responsible for handling all the requests related to the login page
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Role))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password && u.Role == model.Role);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserID", user.UserID.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(claims, "LoginCookie");

                await HttpContext.SignInAsync("LoginCookie", new ClaimsPrincipal(claimsIdentity));

                TempData["Message"] = $"Welcome back, {user.Username}!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password.";
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User newUser)
        {
            ModelState.Remove(nameof(newUser.UserID));

            if (ModelState.IsValid)
            {
                // checks if user or email already exists
                if (_context.Users.Any(u => u.Username == newUser.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(newUser);
                }
                if (_context.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                    return View(newUser);
                }

                if (string.IsNullOrEmpty(newUser.Role))
                {
                    newUser.Role = "Customer"; // default role
                }

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }
            return View(newUser);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("LoginCookie");
            TempData["Message"] = "You have been logged out.";
            return RedirectToAction("Login", "Account");
        }
    }
}
