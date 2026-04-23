using ApiOAuthEmpleados.Data;
using ApiOAuthEmpleados.Helpers;
using ApiOAuthEmpleados.Repositories;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));
});

//ESTE OBJETO SOLO LO NECESITAMOS AQUI, RECUPERAMOS LOS VALORES Y LOS ASIGNAMOS A UNA CLASE
//RECUPERAMOS EL SECRETCLIENT PAR ALOS SECRETOS DE KEYVAULT
SecretClient secretClient =
    builder.Services.BuildServiceProvider()
    .GetService<SecretClient>();
//ACCEDEMOS AL SECRETO
KeyVaultSecret secreto =
    await secretClient.GetSecretAsync("secretsqlazureapd");
//ahora el conection string no se recoje asi
string connectionString = builder.Configuration.GetConnectionString("SqlServer");
//SE HACE cogiendo la clave dle vault
//string connectionString = secreto.Value;

//Creamos una instancia de nuestro helper
HelperActionOAuthService helper = new HelperActionOAuthService(builder.Configuration);
//ESTA INSTANCIA SOLAMENTE DEBEMOS CREARLA UNA VEZ DENTRO DE NUESTRA APLICACION
builder.Services.AddSingleton<HelperActionOAuthService>(helper);
builder.Services.AddTransient<HelperCryptography>();
builder.Services.AddAuthentication(helper.GetAuthenticationSchema())
    .AddJwtBearer(helper.GetJWTBearerOptions());

// Add services to the container.
builder.Services.AddDbContext<HospitalContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddTransient<RepositoryHospital>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
