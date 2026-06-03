using CarRentalSystem.DATA;
using CarRentalSystem.Service.Admin;
using CarRentalSystem.Service.Car;
using CarRentalSystem.Service.Reservation;
using CarRentalSystem.Service.Review;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using CarRentalSystem.Service.Authentication;
using CarRentalSystem.Service.Promotions;
using CarRentalSystem.Service.Maintenances;
using CarRentalSystem.Service.Logistics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

 builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "demowebapi",
                    Version = "v1"
                });
            });

builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAuthentication, AuthenticationService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IMaintenanceService,MaintenanceService>();
builder.Services.AddScoped<IGateLogisticsService, GateLogisticsService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "demowebapi v1");
                    options.RoutePrefix = string.Empty;
                });
    
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("admin123"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("password123"));
app.Run();
