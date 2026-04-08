using ApiOAuthEmpleados.Helpers;
using ApiOAuthEmpleados.Models;
using ApiOAuthEmpleados.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace ApiOAuthEmpleados.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryHospital repo;
        private HelperActionOAuthService helper;
        private HelperEncripter encripter;
        private IConfiguration configuration;
        
        public AuthController(RepositoryHospital repo, HelperActionOAuthService helper, HelperEncripter encripter, IConfiguration configuration)
        {
            this.repo = repo;
            this.helper = helper;
            this.encripter = encripter;
            this.configuration = configuration;
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
                string jsonEmpleado = JsonConvert.SerializeObject(empleado);   
                
                // ENCRIPTAR EL JSON CON NUESTRO HELPER
                string claveEncriptacion = configuration.GetValue<string>("ApiOAuthToken:ClaveEncriptacion");
                string jsonEncriptado = HelperEncripter.EncryptString(jsonEmpleado, claveEncriptacion);

                //CREAMOS UN ARRAY DE CLAIMS PARA EL TOKEN
                Claim[] claims = new[]
                {
                    new Claim("UserData", jsonEncriptado)
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
