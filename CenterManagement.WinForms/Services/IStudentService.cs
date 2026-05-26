using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IStudentService
{
    Task<List<StudentDto>> GetAllAsync(string? search = null);
    Task<StudentDto?> GetByIdAsync(int id);
    Task<List<EnrollmentDto>> GetEnrollmentsAsync(int studentId);
    Task<bool> CreateAsync(CreateStudentDto dto);
    Task<StudentDto?> CreateAndReturnAsync(CreateStudentDto dto);
    Task<bool> UpdateAsync(int id, UpdateStudentDto dto);
}

public class StudentService : IStudentService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<StudentDto>> GetAllAsync(string? search = null)
    {
        var url = string.IsNullOrEmpty(search) ? "/api/students" : $"/api/students?search={Uri.EscapeDataString(search)}";
        return await _api.GetAsync<List<StudentDto>>(url) ?? new();
    }

    public async Task<StudentDto?> GetByIdAsync(int id) =>
        await _api.GetAsync<StudentDto>($"/api/students/{id}");

    public async Task<List<EnrollmentDto>> GetEnrollmentsAsync(int studentId) =>
        await _api.GetAsync<List<EnrollmentDto>>($"/api/students/{studentId}/enrollments") ?? new();

    public async Task<bool> CreateAsync(CreateStudentDto dto)
    {
        var res = await _api.PostRawAsync("/api/students", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<StudentDto?> CreateAndReturnAsync(CreateStudentDto dto)
    {
        try { return await _api.PostAsync<StudentDto>("/api/students", dto); }
        catch { return null; }
    }

    public async Task<bool> UpdateAsync(int id, UpdateStudentDto dto)
    {
        var res = await _api.PutRawAsync($"/api/students/{id}", dto);
        return res.IsSuccessStatusCode;
    }
}
