using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;

namespace CenterManagement.WinForms.Services;

public interface ILeaveRequestService
{
    Task<List<LeaveRequestDto>> GetAllAsync(int? teacherId = null, int? classId = null, LeaveRequestStatus? status = null);
    Task<(bool success, string message)> CreateAsync(CreateLeaveRequestDto dto);
    Task<(bool success, string message)> ApproveAsync(int id, ApproveLeaveDto dto);
}

public class LeaveRequestService : ILeaveRequestService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<LeaveRequestDto>> GetAllAsync(int? teacherId = null, int? classId = null, LeaveRequestStatus? status = null)
    {
        var query = new List<string>();
        if (teacherId.HasValue) query.Add($"teacherId={teacherId}");
        if (classId.HasValue) query.Add($"classId={classId}");
        if (status.HasValue) query.Add($"status={(int)status}");
        var url = "/api/leave-requests" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<List<LeaveRequestDto>>(url) ?? new();
    }

    public async Task<(bool success, string message)> CreateAsync(CreateLeaveRequestDto dto)
    {
        var res = await _api.PostRawAsync("/api/leave-requests", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }

    public async Task<(bool success, string message)> ApproveAsync(int id, ApproveLeaveDto dto)
    {
        var res = await _api.PutRawAsync($"/api/leave-requests/{id}/approve", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }
}
