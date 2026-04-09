using MvcOAuthApiEmpleados.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace MvcOAuthApiEmpleados.Services
{
    public class ServiceEmpleados
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;
        private IHttpContextAccessor contextAccessor;
        public ServiceEmpleados(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
            this.UrlApi = configuration.GetValue<string>
                ("ApiUrls:ApiEmpleados");
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
        }
        public async Task<string> LogInAsync(string user, string pass)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                LoginModel model = new LoginModel
                {
                    UserName = user,
                    Password = pass
                };
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode == true)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return "No se a podido obtener el token";
                }
            }
        }
        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        //REALIZAMOS UNA SOBRECARGA DEL METODO
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            return await this.CallApiAsync<List<Empleado>>("api/empleados");
        }
        //POR AHORA RECIBIREMOS EL TOKEN E EL METODO
        public async Task<Empleado> FindEmpleadoAsync(int id)
        {
            string token = this.contextAccessor.HttpContext.User
                .FindFirst(x => x.Type == "TOKEN").Value;

            return await this.CallApiAsync<Empleado>("api/empleados/" + id, token);
        }
        public async Task<Empleado> GetPerfilAsync()
        {
            string token = this.contextAccessor.HttpContext.User
                .FindFirst(x => x.Type == "TOKEN").Value;
            return await this.CallApiAsync<Empleado>("api/empleados/perfil", token);
        }
        public async Task<List<Empleado>> GetCompisAsync()
        {
            string token = this.contextAccessor.HttpContext.User
                .FindFirst(x => x.Type == "TOKEN").Value;
            return await this.CallApiAsync<List<Empleado>>("api/empleados/compis", token);
        }
        //TANTNO EN INCREMENTAR COMO EN BUSCAR EMPLEADO POR OFICIO NECESITAMOS 
        //GENERAR EL SIGUIENTE STRNG PARA EL REQUEST oficio=ANALISTA&oficio=PROGRAMADOR
        //A PARTIR DE UNA COLECCION
        private string TransformCollectionToQuery(List<string> collection)
        {
            string result = "";
            foreach (string item in collection)
            {
                result += "oficio=" + item + "&";
            }
            return result.TrimEnd('&');
        }
        public async Task<List<Empleado>> GetEmpleadosByOficioAsync(List<string> oficios)
        {
            string request = "api/empleados/empleadosoficios?" + this.TransformCollectionToQuery(oficios);
            return await this.CallApiAsync<List<Empleado>>(request);
        }
        public async Task UpdateEmpleadosAsync(int incremento, List<string> oficios)
        {
            string request = "api/Empleados/IncrementarSalarios/" + incremento;
            string data = this.TransformCollectionToQuery(oficios);
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.PutAsync(request + "?" + data, null);
            }
        }
        public async Task<List<string>> GetOficiosAsync()
        {
            return await this.CallApiAsync<List<string>>("api/empleados/getoficios");

        }
    }
}
