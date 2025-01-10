using ChatBot;
using ChatBot.ChatHistory;
using ChatBot.ChatProviders;
using ChatBot.ChatProviders.OpenAI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web.Resource;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
// builder.Services.AddAuthorization();

// Add http client
builder.Services.AddHttpClient<ChatGptProvider>();


builder.Services.AddScoped<IChatProvider, ChatGptProvider>();

var cosmosClient =
    new CosmosClient(builder.Configuration["CosmosDb:EndPointUri"], builder.Configuration["CosmosDb:PrimaryKey"]);
await cosmosClient.CreateDatabaseIfNotExistsAsync("ChatDb");
var database = cosmosClient.GetDatabase("ChatDb");

builder.Services.AddSingleton<IChatHistory, CosmosChatHistory>(_ => new CosmosChatHistory(database));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}








// app.UseHttpsRedirection();

var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "";
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (HttpContext httpContext) =>
    {
        httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi()
    .RequireAuthorization();

app.MapGet("/chatgpt", async (IChatProvider chatGptService, IChatHistory chatHistory) =>
{
    var response = await chatGptService.SendMessageAsync("write a haiku about ai");
    await chatHistory.AddMessageAsync(new Message(Guid.NewGuid().ToString(), Guid.NewGuid(), MessageAuthor.Bot,
        "message"));
    return Results.Ok(response);
});

app.Run();

app.MapControllers();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}