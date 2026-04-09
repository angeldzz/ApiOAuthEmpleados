using MvcOAuthApiEmpleados.Models;
using Microsoft.AspNetCore.Mvc;
using MvcOAuthApiEmpleados.Filters;
using MvcOAuthApiEmpleados.Services;
using System.Security.Claims;

namespace MvcOAuthApiEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private ServiceEmpleados service;
        public EmpleadosController(ServiceEmpleados service)
        {
            this.service = service;
        }
        [AuthorizeEmpleados]
        public async Task<IActionResult> Index()
        {
            List<Empleado> empleados = await this.service.GetEmpleadosAsync();
            return View(empleados);
        }
        [AuthorizeEmpleados]
        public async Task<IActionResult> Details(int id)
        {
            //TENEDREMOS EL TOKEN EN SESSION
            Empleado empleado = await this.service.FindEmpleadoAsync(id);
            return View(empleado);
        }
        [AuthorizeEmpleados]
        public async Task<IActionResult> Perfil()
        {
            Empleado empleado = await this.service.GetPerfilAsync();
            return View(empleado);
        }
        [AuthorizeEmpleados]
        public async Task<IActionResult> Compis()
        {
            List<Empleado> empleados = await this.service.GetCompisAsync();
            return View(empleados);
        }
        public async Task<IActionResult> EmpleadosOficios()
        {
            List<string> oficios = await this.service.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EmpleadosOficios
            (int? incremento, 
            List<string> oficio,
            string accion)
        {
            List<string> oficios = await this.service.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            if (accion.ToLower() == "update" && incremento != null)
            {
                await this.service.UpdateEmpleadosAsync(incremento.Value, oficio);
            }
            List<Empleado> empleados = await this.service.GetEmpleadosByOficioAsync(oficio);
            return View(empleados);
        }
    }
}
