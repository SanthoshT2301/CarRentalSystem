using CarRentalSystem.DTO.Reports;

namespace CarRentalSystem.Service.Reports;

public interface IReportService
{
    Task<List<BookingReportDto>> GetBookingReportAsync(ReportFilterRequest filter);
    Task<List<RevenueReportDto>> GetRevenueReportAsync(ReportFilterRequest filter);
    Task<List<ReviewReportDto>> GetReviewReportAsync(ReportFilterRequest filter);
    Task<List<CarPerformanceReportDto>> GetPerformanceReportAsync(ReportFilterRequest filter);

    Task<byte[]> ExportBookingReportCsvAsync(ReportFilterRequest filter);
    Task<byte[]> ExportRevenueReportCsvAsync(ReportFilterRequest filter);
    Task<byte[]> ExportReviewReportCsvAsync(ReportFilterRequest filter);
    Task<byte[]> ExportPerformanceReportCsvAsync(ReportFilterRequest filter);
}