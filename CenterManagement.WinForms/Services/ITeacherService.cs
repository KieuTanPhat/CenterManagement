using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface ITeacherService
{
    Task<List<TeacherDto>> GetAllAsync();
    Task<TeacherDto?> GetByIdAsync(int id);
    Task<List<TeacherScheduleDto>> GetScheduleAsync(int teacherId);
    Task<bool> CreateAsync(CreateTeacherDto dto);
    Task<bool> UpdateAsync(int id, UpdateTeacherDto dto);
    Task<bool> ToggleActiveAsync(int id);
}

public class TeacherService : ITeacherService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<TeacherDto>> GetAllAsync() =>
        await _api.GetAsync<List<TeacherDto>>("/api/teachers") ?? new();

    public async Task<TeacherDto?> GetByIdAsync(int id) =>
        await _api.GetAsync<TeacherDto>($"/api/teachers/{id}");

    public async Task<List<TeacherScheduleDto>> GetScheduleAsync(int teacherId) =>
        await _api.GetAsync<List<TeacherScheduleDto>>($"/api/teachers/{teacherId}/schedule") ?? new();

    public async Task<bool> CreateAsync(CreateTeacherDto dto)
    {
        var res = await _api.PostRawAsync("/api/teachers", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateTeacherDto dto)
    {
        var res = await _api.PutRawAsync($"/api/teachers/{id}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var res = await _api.PostRawAsync($"/api/teachers/{id}/toggle-active", new { });
        return res.IsSuccessStatusCode;
    }
}
