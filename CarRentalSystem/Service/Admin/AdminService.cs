using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Admin;
public class AdminService : IAdminService
{
    private readonly AppDbContext _appDbContext;
    public AdminService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<ActionResult<AdminStats>> GetAdminStatsAsync()
    {
            int usersCount = await _appDbContext.Users.CountAsync();
            int carsCount = await _appDbContext.Cars.CountAsync();
            int bookingsCount = await _appDbContext.Reservations.CountAsync();

            decimal totalRevenue = await _appDbContext.Payments
                .Where(p => p.PaymentStatusId == 1)
                .SumAsync(p => p.Amount ?? 0.00m);

            return new AdminStats
            {
                UsersCount = usersCount,
                CarsCount = carsCount,
                BookingsCount = bookingsCount,
                Revenue = totalRevenue
            };
    }
}