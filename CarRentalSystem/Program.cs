using System.Text;
using CarRentalSystem.DATA;
using CarRentalSystem.Middleware;
using CarRentalSystem.Service.Admin;
using CarRentalSystem.Service.Authentication;
using CarRentalSystem.Service.Car;
using CarRentalSystem.Service.Logistics;
using CarRentalSystem.Service.Maintenances;
using CarRentalSystem.Service.Promotions;
using CarRentalSystem.Service.Reservation;
using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── AutoMapper ─────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ── Application Services ────────────────────────────────────────────────────
builder.Services.AddScoped<ICarService,           CarService>();
builder.Services.AddScoped<IReservationService,   ReservationService>();
builder.Services.AddScoped<IReviewService,         ReviewService>();
builder.Services.AddScoped<IAdminService,          AdminService>();
builder.Services.AddScoped<IAuthentication,        AuthenticationService>();
builder.Services.AddScoped<IPromotionService,      PromotionService>();
builder.Services.AddScoped<IMaintenanceService,    MaintenanceService>();
builder.Services.AddScoped<IGateLogisticsService,  GateLogisticsService>();

// ── JWT Authentication ──────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret      = jwtSettings.GetValue<string>("Secret")
                  ?? throw new InvalidOperationException("JWT Secret is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience            = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

builder.Services.AddAuthorization();

// ── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger (with JWT support) ──────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CarRentalSystem API", Version = "v1" });

    // Allow pasting a Bearer token in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── OpenAPI (built-in .NET 9+) ─────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// ── Middleware Pipeline ─────────────────────────────────────────────────────
// 1. Global exception handler — must be first to catch everything below
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CarRentalSystem v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

// 2. Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();