using ApiOAuthEmpleados.Helpers;
using ApiOAuthEmpleados.Models;
using ApiOAuthEmpleados.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiOAuthEmpleados.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryHospital repo;
        private HelperActionOAuthService helper;
        private HelperCryptography encripter;
        public AuthController(RepositoryHospital repo, HelperActionOAuthService helper, HelperCryptography encripter)
        {
            this.repo = repo;
            this.helper = helper;
            this.encripter = encripter;
        }
        [HttpPost]
        [Route ("[action]")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            
            Empleado empleado = await this.repo.LogInEmpleado(model.UserName, int.Parse(model.Password));
            if (empleado == null)
            {
                return Unauthorized();
            }
            else
            {
                EmpleadoModel modelEmp = new EmpleadoModel
                {
                    IdEmpleado = empleado.IdEmpleado,
                    Apellido = empleado.Apellido,
                    Oficio = empleado.Oficio,
                    Salario = empleado.Salario,
                    IdDepartamento = empleado.IdDepartamento
                };

                string jsonEmpleado = JsonConvert.SerializeObject(empleado);   
                
                // ENCRIPTAR EL JSON CON NUESTRO HELPER
                string jsonEncriptado = HelperCryptography.EncryptString(jsonEmpleado, "encriptado123");

                //CREAMOS UN ARRAY DE CLAIMS PARA EL TOKEN
                Claim[] claims = new[]
                {
                    new Claim("UserData", jsonEncriptado),
                    new Claim(ClaimTypes.Role, empleado.Oficio)
                };
                //DEBEMOS CREAR UNAS CREDENCIALES CON NUESTRO TOKEN
                SigningCredentials credentials = 
                    new SigningCredentials
                    (this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);
                //El token se genera con una clase y debemos almacenar los datos de issuer
                JwtSecurityToken token = 
                    new JwtSecurityToken(
                        claims: claims,
                        issuer: this.helper.Issuer,
                        audience: this.helper.Audience,
                        signingCredentials: credentials,
                        expires: DateTime.UtcNow.AddMinutes(20),
                        notBefore: DateTime.UtcNow
                        );
                //POR ULTIMO DEVOLVEMOS LA RESPUESTA AFIRMATIVA CON EL TOKEN
                return Ok(new
                {
                    response = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
        }
    }
}
