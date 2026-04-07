using ApiOAuthEmpleados.Models;
using Microsoft.AspNetCore.Mvc;
using MvcOAuthApiEmpleados.Services;
using System.Threading.Tasks;

namespace MvcOAuthApiEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private ServiceEmpleados service;
        public EmpleadosController(ServiceEmpleados service)
        {
            this.service = service;
        }
        public async Task<IActionResult> Index()
        {
            List<Empleado> empleados = await this.service.GetEmpleadosAsync();
            return View(empleados);
        }
        public async Task<IActionResult> Detalles(int id)
        {
            //TENEDREMOS EL TOKEN EN SESSION
            string token = HttpContext.Session.GetString("TOKEN");
            if (token == null)
            {
                ViewData["MENSAJE"] = "DEBES HACER EL LOGIN";
                return View();
            }
            else
            {
                Empleado empleado = await this.service.FindEmpleadoAsync(id, token);
                return View(empleado);
            }
                
        }
    }
}
