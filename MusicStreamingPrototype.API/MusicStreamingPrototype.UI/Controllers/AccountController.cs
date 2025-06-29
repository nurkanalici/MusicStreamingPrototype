
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MusicStreamingPrototype.UI.Models;
using MusicStreamingPrototype.UI.Services;

namespace MusicStreamingPrototype.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiClient _api;
        public AccountController(IApiClient api) => _api = api;

        
        [HttpGet, AllowAnonymous]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var ok = await _api.RegisterAsync(vm.Username, vm.Password);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty,
                    "Kayıt başarısız, lütfen tekrar deneyin.");
                return View(vm);
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string returnUrl = "/") =>
            View(new LoginViewModel { ReturnUrl = returnUrl });

        
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            
            string? token = await _api.LoginAsync(vm.Username, vm.Password);
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty,
                    "Kullanıcı adı veya parola hatalı.");
                return View(vm);
            }

            
            _api.StoreTokenInSession(token);

            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, vm.Username),
                new Claim("Jwt", token),
                new Claim(ClaimTypes.Role, "User")
            };
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "CookieAuth"));

            
            await HttpContext.SignInAsync(
                "CookieAuth",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                });

            
            return LocalRedirect(string.IsNullOrEmpty(vm.ReturnUrl) ? "/" : vm.ReturnUrl);
        }

        
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction(nameof(Login));
        }
    }
}
