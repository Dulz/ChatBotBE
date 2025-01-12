using ChatBot.Chat;
using ChatBot.ChatHistory;
using ChatBot.ChatHistory.Cosmos;
using ChatBot.ChatProviders;
using ChatBot.ChatProviders.OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
await ConfigureServices(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

return;

async Task ConfigureServices(WebApplicationBuilder webApplicationBuilder)
{
    // Add authorization
    webApplicationBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(webApplicationBuilder.Configuration.GetSection("AzureAdB2C"),
            subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);
    webApplicationBuilder.Services.AddAuthorization();

    // Development dependencies
    if (webApplicationBuilder.Environment.IsDevelopment())
        // Allow CORS
        webApplicationBuilder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

    webApplicationBuilder.Services.AddControllers();

    webApplicationBuilder.Services.AddHttpClient<ChatGptProvider>();

    // DI configuration
    webApplicationBuilder.Services.AddScoped<IChatProvider, ChatGptProvider>();
    webApplicationBuilder.Services.AddScoped<ChatService>();
    var database = await CosmosDbFactory.GetDatabase(webApplicationBuilder.Configuration);
    webApplicationBuilder.Services.AddSingleton<IChatHistory, CosmosChatHistory>(_ => new CosmosChatHistory(database));
}