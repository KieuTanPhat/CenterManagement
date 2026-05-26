using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync(string? search = null, int? branchId = null);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<bool> CreateAsync(CreateEmployeeDto dto);
    Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<bool> ToggleActiveAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<EmployeeDto>> GetAllAsync(string? search = null, int? branchId = null)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(search)) q.Add($"search={Uri.EscapeDataString(search)}");
        if (branchId.HasValue) q.Add($"branchId={branchId}");
        var url = "/api/employees" + (q.Any() ? "?" + string.Join("&", q) : "");
        return await _api.GetAsync<List<EmployeeDto>>(url) ?? new();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id) =>
        await _api.GetAsync<EmployeeDto>($"/api/employees/{id}");

    public async Task<bool> CreateAsync(CreateEmployeeDto dto)
    {
        var res = await _api.PostRawAsync("/api/employees", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        var res = await _api.PutRawAsync($"/api/employees/{id}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var res = await _api.PostRawAsync($"/api/employees/{id}/toggle-active", new { });
        return res.IsSuccessStatusCode;
    }
}
