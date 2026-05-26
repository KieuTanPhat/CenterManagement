using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IReportService
{
    Task<DashboardDto?> GetDashboardAsync();
    Task<RevenueReportDto?> GetRevenueAsync(int? month = null, int? year = null);
    Task<TeacherSalaryDto?> GetTeacherSalaryAsync(int teacherId, int classId);
}

public class ReportService : IReportService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<DashboardDto?> GetDashboardAsync() =>
        await _api.GetAsync<DashboardDto>("/api/reports/dashboard");

    public async Task<RevenueReportDto?> GetRevenueAsync(int? month = null, int? year = null)
    {
        var query = new List<string>();
        if (month.HasValue) query.Add($"month={month}");
        if (year.HasValue) query.Add($"year={year}");
        var url = "/api/reports/revenue" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<RevenueReportDto>(url);
    }

    public async Task<TeacherSalaryDto?> GetTeacherSalaryAsync(int teacherId, int classId) =>
        await _api.GetAsync<TeacherSalaryDto>($"/api/reports/teacher-salary/{teacherId}/{classId}");
}
