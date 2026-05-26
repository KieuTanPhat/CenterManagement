using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IAttendanceService
{
    Task<List<AttendanceDto>> GetByScheduleAsync(int scheduleId);
    Task<List<AttendanceSummaryDto>> GetClassSummaryAsync(int classId);
    Task<(bool success, string message)> BulkMarkAsync(BulkAttendanceDto dto);
}

public class AttendanceService : IAttendanceService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<AttendanceDto>> GetByScheduleAsync(int scheduleId) =>
        await _api.GetAsync<List<AttendanceDto>>($"/api/attendances?scheduleId={scheduleId}") ?? new();

    public async Task<List<AttendanceSummaryDto>> GetClassSummaryAsync(int classId) =>
        await _api.GetAsync<List<AttendanceSummaryDto>>($"/api/attendances/summary/{classId}") ?? new();

    public async Task<(bool success, string message)> BulkMarkAsync(BulkAttendanceDto dto)
    {
        var res = await _api.PostRawAsync("/api/attendances/bulk", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }
}
