using System.Text;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Reports;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context) => _context = context;

    // ── Date helpers ──────────────────────────────────────────────────────────
    private static (DateTime start, DateTime end) ParseRange(ReportFilterRequest filter)
    {
        var start = DateTime.TryParse(filter.StartDate, out var s) ? s : DateTime.UtcNow.AddMonths(-1);
        var end = DateTime.TryParse(filter.EndDate, out var e) ? e : DateTime.UtcNow;
        return (start.Date, end.Date.AddDays(1).AddTicks(-1)); // inclusive end
    }

    // ── BOOKING REPORT ────────────────────────────────────────────────────────
    public async Task<List<BookingReportDto>> GetBookingReportAsync(ReportFilterRequest filter)
    {
        var (start, end) = ParseRange(filter);

        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Car).ThenInclude(c => c!.Brand)
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .Include(r => r.Car)
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new BookingReportDto
            {
                ReservationId = r.ReservationId,
                CustomerName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "N/A",
                CustomerEmail = r.User != null ? r.User.Email : "N/A",
                CarName = r.Car != null ? $"{(r.Car.Brand != null ? r.Car.Brand.BrandName : "")} {r.Car.Model}" : "N/A",
                PickupLocation = r.PickupLocation != null ? r.PickupLocation.LocationName : "N/A",
                DropoffLocation = r.DropoffLocation != null ? r.DropoffLocation.LocationName : "N/A",
                PickupDate = r.PickupDate.ToString("yyyy-MM-dd"),
                DropoffDate = r.DropDate.ToString("yyyy-MM-dd"),
                TotalAmount = r.TotalAmount ?? 0,
                Status = r.ReservationStatus != null ? r.ReservationStatus.StatusName : "N/A",
                CreatedAt = r.CreatedAt.HasValue ? r.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm") : "N/A"
            })
            .ToListAsync();
    }

    // ── REVENUE REPORT ────────────────────────────────────────────────────────
    public async Task<List<RevenueReportDto>> GetRevenueReportAsync(ReportFilterRequest filter)
    {
        var (start, end) = ParseRange(filter);

        var payments = await _context.Payments
            .Include(p => p.Reservation)
            .Include(p => p.PaymentStatus)
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
            .ToListAsync();

        var reservations = await _context.Reservations
            .Include(r => r.Car).ThenInclude(c => c!.Brand)
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
            .ToListAsync();

        // Group by year-month
        var grouped = payments
            .GroupBy(p => p.PaymentDate.HasValue
                ? p.PaymentDate.Value.ToString("yyyy-MM")
                : "Unknown")
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var periodReservations = reservations
                    .Where(r => r.CreatedAt.HasValue &&
                                r.CreatedAt.Value.ToString("yyyy-MM") == g.Key)
                    .ToList();

                var gross = g.Where(p => p.PaymentStatusId == 1).Sum(p => p.Amount ?? 0);
                var refunds = g.Where(p => p.PaymentStatusId == 3).Sum(p => p.Amount ?? 0);

                // Top earning car for this period
                var topCar = periodReservations
                    .GroupBy(r => r.Car != null
                        ? $"{(r.Car.Brand != null ? r.Car.Brand.BrandName : "")} {r.Car.Model}"
                        : "Unknown")
                    .OrderByDescending(cg => cg.Sum(r => r.TotalAmount ?? 0))
                    .FirstOrDefault()?.Key ?? "N/A";

                return new RevenueReportDto
                {
                    Period = g.Key,
                    TotalBookings = periodReservations.Count,
                    CompletedRentals = periodReservations.Count(r => r.ReservationStatusId == 2),
                    CancelledRentals = periodReservations.Count(r => r.ReservationStatusId == 3),
                    GrossRevenue = gross,
                    Refunds = refunds,
                    NetRevenue = gross - refunds,
                    TopEarningCar = topCar
                };
            })
            .ToList();

        return grouped;
    }

    // ── REVIEW REPORT ─────────────────────────────────────────────────────────
    public async Task<List<ReviewReportDto>> GetReviewReportAsync(ReportFilterRequest filter)
    {
        var (start, end) = ParseRange(filter);

        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation).ThenInclude(res => res!.Car).ThenInclude(c => c!.Brand)
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewReportDto
            {
                ReviewId = r.ReviewId,
                CustomerName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "N/A",
                CarName = r.Reservation != null && r.Reservation.Car != null
                                        ? $"{(r.Reservation.Car.Brand != null ? r.Reservation.Car.Brand.BrandName : "")} {r.Reservation.Car.Model}"
                                        : "N/A",
                Rating = r.Rating ?? 0,
                Comment = r.Comment ?? string.Empty,
                IsDisputed = r.IsDisputed,
                DisputeResolution = r.DisputeResolution ?? string.Empty,
                CreatedAt = r.CreatedAt.HasValue ? r.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm") : "N/A"
            })
            .ToListAsync();
    }

    // ── PERFORMANCE REPORT ────────────────────────────────────────────────────
    public async Task<List<CarPerformanceReportDto>> GetPerformanceReportAsync(ReportFilterRequest filter)
    {
        var (start, end) = ParseRange(filter);

        var cars = await _context.Cars
            .Include(c => c.Brand)
            .Include(c => c.Location)
            .Include(c => c.CarStatus)
            .ToListAsync();

        var reservations = await _context.Reservations
            .Include(r => r.Car)
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end && p.PaymentStatusId == 1)
            .ToListAsync();

        var reviews = await _context.Reviews
            .Include(r => r.Reservation)
            .Where(r => r.CreatedAt >= start && r.CreatedAt <= end && r.Rating != null)
            .ToListAsync();

        var totalDays = Math.Max(1, (end - start).TotalDays);

        return cars.Select(car =>
        {
            var carReservations = reservations.Where(r => r.CarId == car.CarId).ToList();
            var carPayments = payments.Where(p => p.Reservation != null && p.Reservation.CarId == car.CarId).ToList();
            var carReviews = reviews.Where(r => r.Reservation != null && r.Reservation.CarId == car.CarId).ToList();

            var rentedDays = carReservations
                .Where(r => r.ReservationStatusId == 2)
                .Sum(r => Math.Max(1, (r.DropDate - r.PickupDate).TotalDays));

            return new CarPerformanceReportDto
            {
                CarId = car.CarId,
                CarName = $"{(car.Brand != null ? car.Brand.BrandName : "")} {car.Model}",
                Location = car.Location != null ? car.Location.LocationName : "N/A",
                Status = car.CarStatus != null ? car.CarStatus.StatusName : "N/A",
                TotalBookings = carReservations.Count,
                CompletedRentals = carReservations.Count(r => r.ReservationStatusId == 2),
                CancelledRentals = carReservations.Count(r => r.ReservationStatusId == 3),
                TotalRevenue = carPayments.Sum(p => p.Amount ?? 0),
                AverageRating = carReviews.Any() ? Math.Round(carReviews.Average(r => r.Rating!.Value), 1) : 0,
                ReviewCount = carReviews.Count,
                UtilizationRate = Math.Round(rentedDays / totalDays * 100, 1)
            };
        })
        .OrderByDescending(c => c.TotalRevenue)
        .ToList();
    }

    // ── CSV EXPORTS ───────────────────────────────────────────────────────────
    public async Task<byte[]> ExportBookingReportCsvAsync(ReportFilterRequest filter)
    {
        var data = await GetBookingReportAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("ReservationId,CustomerName,CustomerEmail,Car,PickupLocation,DropoffLocation,PickupDate,DropoffDate,TotalAmount,Status,CreatedAt");

        foreach (var r in data)
            sb.AppendLine($"{r.ReservationId}," +
                          $"\"{r.CustomerName}\"," +
                          $"\"{r.CustomerEmail}\"," +
                          $"\"{r.CarName}\"," +
                          $"\"{r.PickupLocation}\"," +
                          $"\"{r.DropoffLocation}\"," +
                          $"{r.PickupDate}," +
                          $"{r.DropoffDate}," +
                          $"{r.TotalAmount}," +
                          $"{r.Status}," +
                          $"{r.CreatedAt}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportRevenueReportCsvAsync(ReportFilterRequest filter)
    {
        var data = await GetRevenueReportAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("Period,TotalBookings,CompletedRentals,CancelledRentals,GrossRevenue,Refunds,NetRevenue,TopEarningCar");

        foreach (var r in data)
            sb.AppendLine($"{r.Period}," +
                          $"{r.TotalBookings}," +
                          $"{r.CompletedRentals}," +
                          $"{r.CancelledRentals}," +
                          $"{r.GrossRevenue}," +
                          $"{r.Refunds}," +
                          $"{r.NetRevenue}," +
                          $"\"{r.TopEarningCar}\"");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportReviewReportCsvAsync(ReportFilterRequest filter)
    {
        var data = await GetReviewReportAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("ReviewId,CustomerName,Car,Rating,Comment,IsDisputed,DisputeResolution,CreatedAt");

        foreach (var r in data)
            sb.AppendLine($"{r.ReviewId}," +
                          $"\"{r.CustomerName}\"," +
                          $"\"{r.CarName}\"," +
                          $"{r.Rating}," +
                          $"\"{EscapeCsv(r.Comment)}\"," +
                          $"{r.IsDisputed}," +
                          $"\"{EscapeCsv(r.DisputeResolution)}\"," +
                          $"{r.CreatedAt}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportPerformanceReportCsvAsync(ReportFilterRequest filter)
    {
        var data = await GetPerformanceReportAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("CarId,CarName,Location,Status,TotalBookings,CompletedRentals,CancelledRentals,TotalRevenue,AverageRating,ReviewCount,UtilizationRate%");

        foreach (var r in data)
            sb.AppendLine($"{r.CarId}," +
                          $"\"{r.CarName}\"," +
                          $"\"{r.Location}\"," +
                          $"{r.Status}," +
                          $"{r.TotalBookings}," +
                          $"{r.CompletedRentals}," +
                          $"{r.CancelledRentals}," +
                          $"{r.TotalRevenue}," +
                          $"{r.AverageRating}," +
                          $"{r.ReviewCount}," +
                          $"{r.UtilizationRate}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value) =>
        value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
}