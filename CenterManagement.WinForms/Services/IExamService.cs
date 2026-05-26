using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IExamService
{
    Task<List<ExamDto>> GetByClassAsync(int classId);
    Task<List<ExamResultDto>> GetResultsAsync(int examId);
    Task<bool> CreateExamAsync(CreateExamDto dto);
    Task<(bool success, string message)> SubmitResultsAsync(BulkExamResultDto dto);
}

public class ExamService : IExamService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<ExamDto>> GetByClassAsync(int classId) =>
        await _api.GetAsync<List<ExamDto>>($"/api/exams?classId={classId}") ?? new();

    public async Task<List<ExamResultDto>> GetResultsAsync(int examId) =>
        await _api.GetAsync<List<ExamResultDto>>($"/api/exams/{examId}/results") ?? new();

    public async Task<bool> CreateExamAsync(CreateExamDto dto)
    {
        var res = await _api.PostRawAsync("/api/exams", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<(bool success, string message)> SubmitResultsAsync(BulkExamResultDto dto)
    {
        var res = await _api.PostRawAsync($"/api/exams/{dto.ExamId}/results", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }
}
