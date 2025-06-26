using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using SalesManagement.Api.Authorization;
using SalesManagement.Api.Controllers.Parameters;
using SalesManagement.Bussiness;
using SalesManagement.Common.Helper;
using SalesManagement.Common.Model;
using SalesManagement.Common.Supports;
using SalesManagement.Entities.Data;
using SalesManagement.Entities.Enum;
using System.Text;
using System.Text.Json;
using AppSettings = SalesManagement.Common.Model.AppSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "SaleManagementApi", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Define logRepository before using it
ILoggerRepository logRepository = log4net.LogManager.GetRepository();
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));




builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var secretKey = builder.Configuration["AppSettings:SecretKey"];
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("SecretKey cannot be null or empty.");
    }
    var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<SalesManagement.Nhibernate.SessionManager>();
builder.Services.AddScoped<UserServiceBll>();
builder.Services.AddScoped<UserParameters>(provider =>
{
    return new UserParameters
    {
        UserService = provider.GetRequiredService<UserServiceBll>()
    };
});
// Replace the existing ISessionFactory registration with:
builder.Services.AddSingleton<ISessionFactory>(provider =>
{
    var cfg = new Configuration();
    cfg.Configure();
    cfg.AddAssembly(typeof(User).Assembly);
    // Use AsyncLocalSessionContext instead of WebSessionContext
    cfg.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass,
        typeof(NHibernate.Context.AsyncLocalSessionContext).FullName);
    return cfg.BuildSessionFactory();
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddAutoMapper(typeof(ApplicationMapper));

// Add after the existing service registrations
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<JwtToken>();

// Add after the existing service registrations
builder.Services.AddScoped<AuthServiceBll>();
builder.Services.AddScoped<AuthParameters>(provider =>
{
    return new AuthParameters
    {
        AuthService = provider.GetRequiredService<AuthServiceBll>()
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SaleManagementApi v1");
    c.RoutePrefix = "swagger";

    // Ẩn phần Models (Schemas)
    c.DefaultModelsExpandDepth(-1); // <= dòng quan trọng
});


app.UseHttpsRedirection();

// Add after AddAuthentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
