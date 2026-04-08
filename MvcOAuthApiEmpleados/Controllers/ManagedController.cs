using MvcOAuthApiEmpleados.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MvcOAuthApiEmpleados.Services;
using System.Net;
using System.Security.Claims;

namespace MvcOAuthApiEmpleados.Controllers
{
    public class ManagedController : Controller
    {
        private ServiceEmpleados service;
        public ManagedController(ServiceEmpleados service)
        {
            this.service = service;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            string token = await this.service.LogInAsync(model.UserName, model.Password);
            if (token == null)
            {
                ViewData["MENSAJE"] = "Credenciales incorrectas";
            }
            else
            {
                ViewData["MENSAJE"] = "Tienes tu token!!";
                HttpContext.Session.SetString("TOKEN", token);
                ClaimsIdentity identity = new ClaimsIdentity
                    (CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                //Almacenamos por ahora el nombre del usuario
                identity.AddClaim(new Claim(ClaimTypes.Name, model.UserName));
                identity.AddClaim(new Claim("TOKEN", token));
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                //Damos de alta al usuario durante 20 minutos
                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20)
                    });
            }
            return RedirectToAction("Index", "Empleados");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
