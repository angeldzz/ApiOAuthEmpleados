using ApiOAuthEmpleados.Helpers;
using ApiOAuthEmpleados.Models;
using ApiOAuthEmpleados.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ApiOAuthEmpleados.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryHospital repo;
        private HelperActionOAuthService helper;
        public AuthController(RepositoryHospital repo, HelperActionOAuthService helper)
        {
            this.repo = repo;
            this.helper = helper;
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
                //DEBEMOS CREAR UNAS CREDENCIALES CON NUESTRO TOKEN
                SigningCredentials credentials = 
                    new SigningCredentials
                    (this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);
                //El token se genera con una clase y debemos almacenar los datos de issuer
                JwtSecurityToken token = 
                    new JwtSecurityToken(
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
