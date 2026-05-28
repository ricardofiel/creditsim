using System.Text.Json;
using System.Text.Json.Serialization;
using CreditSim.Core.Services;
using CreditSim.Data;
using CreditSim.Data.Repositories;
using CreditSim.Web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON options that mirror the original Web API 2 setup
// (camelCase property names, ignore nulls, UTC date times).
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// CORS — allow all origins (mirrors the original EnableCorsAttribute("*", "*", "*")).
const string AllowAllCors = nameof(AllowAllCors);
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllCors, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Domain services.
builder.Services.AddSingleton<ICreditScoringService, CreditScoringService>();
builder.Services.AddSingleton<ICustomerRepository>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var config = sp.GetRequiredService<IConfiguration>();
    var path = ConnectionStringProvider.Get(config, env);
    return new CustomerRepository(path);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors(AllowAllCors);
app.MapControllers();

// Initialise the customer store (creates the JSON file with seed data if missing).
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var scoring = scope.ServiceProvider.GetRequiredService<ICreditScoringService>();
    var storePath = ConnectionStringProvider.Get(config, env);
    var initializer = new DatabaseInitializer(storePath, scoring);

    try
    {
        await initializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Customer store initialisation failed");
    }
}

app.Run();

// Expose the implicit Program type for WebApplicationFactory in integration tests.
public partial class Program { }
