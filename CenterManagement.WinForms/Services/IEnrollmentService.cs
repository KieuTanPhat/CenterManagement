using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IEnrollmentService
{
    Task<List<EnrollmentDto>> GetAllAsync(int? classId = null, int? studentId = null);
    Task<(bool success, string message, object? data)> EnrollAsync(CreateEnrollmentDto dto);
    Task<(bool success, string message)> CancelAsync(int id, CancelEnrollmentDto dto);
    Task<(bool success, string message)> TransferAsync(int id, TransferEnrollmentDto dto);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<EnrollmentDto>> GetAllAsync(int? classId = null, int? studentId = null)
    {
        var query = new List<string>();
        if (classId.HasValue) query.Add($"classId={classId}");
        if (studentId.HasValue) query.Add($"studentId={studentId}");
        var url = "/api/enrollments" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<List<EnrollmentDto>>(url) ?? new();
    }

    public async Task<(bool success, string message, object? data)> EnrollAsync(CreateEnrollmentDto dto)
    {
        var res = await _api.PostRawAsync("/api/enrollments", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg, null);
    }

    public async Task<(bool success, string message)> CancelAsync(int id, CancelEnrollmentDto dto)
    {
        var res = await _api.PostRawAsync($"/api/enrollments/{id}/cancel", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }

    public async Task<(bool success, string message)> TransferAsync(int id, TransferEnrollmentDto dto)
    {
        var res = await _api.PostRawAsync($"/api/enrollments/{id}/transfer", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }
}
