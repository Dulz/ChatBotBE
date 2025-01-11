using ChatBot;
using ChatBot.Chat;
using ChatBot.ChatHistory;
using ChatBot.ChatHistory.Cosmos;
using ChatBot.ChatProviders;
using ChatBot.ChatProviders.OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"), subscribeToJwtBearerMiddlewareDiagnosticsEvents:true);
builder.Services.AddAuthorization();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

// Add http client
builder.Services.AddHttpClient<ChatGptProvider>();

builder.Services.AddControllers();

builder.Services.AddScoped<IChatProvider, ChatGptProvider>();

var cosmosClient =
    new CosmosClient(builder.Configuration["CosmosDb:EndPointUri"], builder.Configuration["CosmosDb:PrimaryKey"]);
await cosmosClient.CreateDatabaseIfNotExistsAsync("ChatDb");
var database = cosmosClient.GetDatabase("ChatDb");

builder.Services.AddSingleton<IChatHistory, CosmosChatHistory>(_ => new CosmosChatHistory(database));

builder.Services.AddScoped<ChatService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Enable CORS
app.UseCors("AllowAllOrigins");

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();

