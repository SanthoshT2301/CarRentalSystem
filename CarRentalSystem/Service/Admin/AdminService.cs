using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Admin;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context) => _context = context;

    public async Task<ActionResult<AdminStats>> GetAdminStatsAsync()
    {
        var usersCount = await _context.Users.CountAsync();
        var carsCount = await _context.Cars.CountAsync();
        var bookingsCount = await _context.Reservations.CountAsync();
        var totalRevenue = await _context.Payments
            .Where(p => p.PaymentStatusId == 1)
            .SumAsync(p => p.Amount ?? 0m);

        return new AdminStats
        {
            UsersCount = usersCount,
            CarsCount = carsCount,
            BookingsCount = bookingsCount,
            Revenue = totalRevenue
        };
    }
}