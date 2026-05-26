using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;

namespace CenterManagement.WinForms.Services;

public interface IClassService
{
    Task<List<ClassDto>> GetAllAsync(ClassStatus? status = null, int? courseId = null);
    Task<ClassDetailDto?> GetByIdAsync(int id);
    Task<bool> CreateAsync(CreateClassDto dto);
    Task<bool> UpdateAsync(int id, UpdateClassDto dto);
    Task<(bool success, string message)> ConfirmStartAsync(int id);
    Task<(bool success, string message)> CancelAsync(int id);
    Task<(bool success, string message)> AssignTeacherAsync(int classId, int teacherId, bool isMain = true);
}

public class ClassService : IClassService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<ClassDto>> GetAllAsync(ClassStatus? status = null, int? courseId = null)
    {
        var query = new List<string>();
        if (status.HasValue) query.Add($"status={(int)status.Value}");
        if (courseId.HasValue) query.Add($"courseId={courseId.Value}");
        var url = "/api/classes" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<List<ClassDto>>(url) ?? new();
    }

    public async Task<ClassDetailDto?> GetByIdAsync(int id) =>
        await _api.GetAsync<ClassDetailDto>($"/api/classes/{id}");

    public async Task<bool> CreateAsync(CreateClassDto dto)
    {
        var res = await _api.PostRawAsync("/api/classes", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateClassDto dto)
    {
        var res = await _api.PutRawAsync($"/api/classes/{id}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<(bool success, string message)> ConfirmStartAsync(int id)
    {
        var res = await _api.PostRawAsync($"/api/classes/{id}/confirm-start", new { });
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }

    public async Task<(bool success, string message)> CancelAsync(int id)
    {
        var res = await _api.PostRawAsync($"/api/classes/{id}/cancel", new { });
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }

    public async Task<(bool success, string message)> AssignTeacherAsync(int classId, int teacherId, bool isMain = true)
    {
        var res = await _api.PostRawAsync($"/api/classes/{classId}/assign-teacher", new { TeacherId = teacherId, IsMainTeacher = isMain });
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }
}
