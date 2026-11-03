
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using RentEase.DTOs;
using RentEase.Interfaces;

namespace RentEase.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: Register
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _accountService.RegisterLandlordAsync(model);

            if (user == null)
            {
                ModelState.AddModelError(
                    "Email", "This email is already registered.");

                return View(model);
            }

            TempData["Success"] = "Registration successful. Please log in.";

            return RedirectToAction("LandlordLogin");
        }

        // GET: Login - Login page selection
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // GET: Landlord Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult LandlordLogin()
        {
            return View();
        }

        // POST: Landlord Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LandlordLogin(LoginDTO model)
        {
            return await ProcessLogin(model, "Landlord", "LandlordLogin");
        }

        // GET: Tenant Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult TenantLogin()
        {
            return View();
        }

        // POST: Tenant Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TenantLogin(LoginDTO model)
        {
            return await ProcessLogin(model, "Tenant", "TenantLogin");
        }

        // Common login processing
        private async Task<IActionResult> ProcessLogin(
            LoginDTO model,
            string requiredRole,
            string viewName)
        {
            if (!ModelState.IsValid)
                return View(viewName, model);

            var user = await _accountService.ValidateLoginAsync(model);

            // Check credentials AND account role
            if (user == null || user.Role != requiredRole)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password."
                );

                return View(viewName, model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            if (user.Role == "Tenant" && user.MustChangePassword)
            {
                return RedirectToAction("ChangePassword");
            }

            if (user.Role == "Tenant")
            {
                return RedirectToAction("TenantDashboard", "Rental");
            }

            return RedirectToAction("Dashboard", "Property");
        }

        // GET: Create Tenant
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public IActionResult CreateTenant()
        {
            return View();
        }

        // POST: Create Tenant
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTenant(TenantCreateDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int landlordId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            string? temporaryPassword =
                await _accountService.CreateTenantAsync(model, landlordId);

            if (temporaryPassword == null)
            {
                ModelState.AddModelError(
                    "Email", "Email already exists or landlord account is invalid.");

                return View(model);
            }

            TempData["TenantName"] = model.FullName;
            TempData["TenantEmail"] = model.Email;
            TempData["TemporaryPassword"] = temporaryPassword;

            return RedirectToAction("TenantCreated");
        }

        // GET: Tenant Created
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public IActionResult TenantCreated()
        {
            if (TempData["TemporaryPassword"] == null)
                return RedirectToAction("CreateTenant");

            return View();
        }
        // GET: Change Password
        [Authorize(Roles = "Tenant")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Change Password
        [Authorize(Roles = "Tenant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            bool result = await _accountService.ChangePasswordAsync(userId, model);

            if (!result)
            {
                ModelState.AddModelError("", "Unable to change password. Please try again.");
                return View(model);
            }

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Success"] = "Password changed successfully. Please log in again.";

            return RedirectToAction("TenantLogin");
        }
        // Logout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "Account");
        }
    }
}
