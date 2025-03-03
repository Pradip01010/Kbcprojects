using Kbcprojects.Entities;
using Kbcprojects.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BCrypt.Net;

namespace Kbcprojects.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

     
        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }

        // ✅ GET: Show Registration Form
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.Errors = errors;
                return View(model);
            }

            try
            {
                UserAccount account = new UserAccount
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password) // Hash Password
                };

                _context.UserAccounts.Add(account);
                _context.SaveChanges();

                ViewBag.Message = $"{account.FirstName} {account.LastName} registered successfully. Please login.";
                ModelState.Clear();
                return View();
            }
            catch (DbUpdateException ex)
            {
                ViewBag.Error = "Error registering user. Please enter a unique email or username.";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error registering user. Please try again.";
                return View(model);
            }
        }




        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.UserAccounts.FirstOrDefaultAsync(x =>
                    x.UserName == model.UserNameOrEmail || x.Email == model.UserNameOrEmail);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                   
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("Name", user.FirstName),
                        new Claim("UserId", user.id.ToString()), 
                        new Claim(ClaimTypes.Role, "User") 
                    };

                   
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToAction("SecurePage");
                }

                ModelState.AddModelError("", "Invalid Username/Email or Password.");
            }

            return View(model);
        }

       
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

       
        [Authorize]
        public IActionResult SecurePage()
        {
            ViewBag.Name = User.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;
            return View();
        }
    }
}

