using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface ICourseService
{
    Task<List<CourseDto>> GetAllAsync(bool activeOnly = false);
    Task<CourseDto?> GetByIdAsync(int id);
    Task<bool> CreateAsync(CreateCourseDto dto);
    Task<bool> UpdateAsync(int id, UpdateCourseDto dto);
}

public class CourseService : ICourseService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<CourseDto>> GetAllAsync(bool activeOnly = false)
    {
        var url = activeOnly ? "/api/courses?activeOnly=true" : "/api/courses";
        return await _api.GetAsync<List<CourseDto>>(url) ?? new();
    }

    public async Task<CourseDto?> GetByIdAsync(int id) =>
        await _api.GetAsync<CourseDto>($"/api/courses/{id}");

    public async Task<bool> CreateAsync(CreateCourseDto dto)
    {
        var res = await _api.PostRawAsync("/api/courses", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCourseDto dto)
    {
        var res = await _api.PutRawAsync($"/api/courses/{id}", dto);
        return res.IsSuccessStatusCode;
    }
}
