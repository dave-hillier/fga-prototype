using System.Text.Json;
using Fga.Api.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    //configuration.GetSection("Authentication").Bind(options);
    options.Authority = configuration["Authentication:Authority"];
    options.Audience = configuration["Authentication:Audience"];
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var authorizationModel = JsonSerializer.Deserialize<AuthorizationModel>(File.ReadAllText("model.json"));
builder.Services.AddSingleton(new AuthorizationSystem(authorizationModel));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow()
            {
                Scopes = new Dictionary<string, string>
                {
                    { "write:tuples", "Write Permissions Tuple" }
                },
                AuthorizationUrl = new Uri(configuration["Authentication:Authority"] + "authorize?audience=" + configuration["Authentication:Audience"]),
                TokenUrl = new Uri($"{configuration["Authentication:Authority"]}oauth/token")
            },
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{ 
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },new List<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(configuration["Authentication:ClientId"]);
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();