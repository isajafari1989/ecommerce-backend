using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Configuration;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using ECommerce.Api.Middleware;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var jwtSettings = JwtSettingsConfiguration.GetRequired(builder.Configuration);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. " +
        "Set ConnectionStrings:DefaultConnection in configuration or the ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddControllers();
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)
            )
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    options.IncludeXmlComments(
        Path.Combine(AppContext.BaseDirectory, xmlFilename)
    );

    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ECommerce API",
        Version = "v1",
        Description = "Clean Architecture ECommerce API with Products CRUD",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Developer",
            Email = "developer@example.com"
        }
    });
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
		{
    	Name = "Authorization",
    	Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    	Scheme = "Bearer",
    	BearerFormat = "JWT",
	    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
	    Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOi..."
	});

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
	    {
        	new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        	{
            	Reference = new Microsoft.OpenApi.Models.OpenApiReference
            	{
                	Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                	Id = "Bearer"
            	}
        	},
        	Array.Empty<string>()
    	}
	});

});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAdminUserAsync(context, app.Configuration);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API v1");
        c.DisplayRequestDuration();
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
