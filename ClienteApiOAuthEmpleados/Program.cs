// See https://aka.ms/new-console-template for more information
using ClienteApiOAuthEmpleados;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
Console.WriteLine("Client Api OAuth");
Console.WriteLine("Introduzca Apellido");
string apellido = Console.ReadLine();
Console.WriteLine("Introduzca IdEmpleado");
string password = Console.ReadLine();
string token = await GetTokenAsync(apellido,password);
Console.WriteLine("RESPONSE: " + token);
Console.WriteLine("Introduzca IdEmpleado a buscar");
int idEmpleado = int.Parse(Console.ReadLine());
string respuesta = await FindEmpleadoAsync(idEmpleado, token);
Console.WriteLine("RESPUESTA: " + respuesta);
//PARA CREAR METODOS EN PROGRAM DEBEMOS HACERLOS STATIC
static async Task<string> GetTokenAsync(string user, string pass) 
{
    string urlApi = "https://apioauthempleadosapd.azurewebsites.net/";
    LoginModel model = new LoginModel
    {
        UserName = user,
        Password = pass
    };
    using (HttpClient client = new HttpClient())
    {
        string request = "api/auth/login";
        client.BaseAddress = new Uri(urlApi);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        string json = JsonConvert.SerializeObject(model);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(request, content);
        if (response.IsSuccessStatusCode == true)
        {
            string data = await response.Content.ReadAsStringAsync();
            JObject objeto = JObject.Parse(data);
            string token = objeto.GetValue("response").ToString();
            return token;
        }
        else
        {
            return "Peticion Incorrecta: " + response.StatusCode;
        }
    }
}
static async Task<string> FindEmpleadoAsync(int idEmpleado, string token)
{
    string urlApi = "https://apioauthempleadosapd.azurewebsites.net/";

    using (HttpClient client = new HttpClient())
    {
        string request = "api/empleados/" + idEmpleado;
        client.BaseAddress = new Uri(urlApi);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new
            MediaTypeWithQualityHeaderValue("application/json"));
        //Lo unico de novedad es incluir authorization con bearer TOKEN dentro del header
        client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
        HttpResponseMessage response = await client.GetAsync(request);
        if (response.IsSuccessStatusCode == true)
        {
            string data = await response.Content.ReadAsStringAsync();
            return data;
        }
        else
        {
            return "Error de algo: " + response.StatusCode;
        }
    }
}