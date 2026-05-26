using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IScheduleService
{
    Task<List<ScheduleDto>> GetAllAsync(int? classId = null, int? roomId = null, DateOnly? date = null);
    Task<List<TimeSlotDto>> GetTimeSlotsAsync();
    Task<bool> CreateAsync(CreateScheduleDto dto);
}

public class ScheduleService : IScheduleService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<ScheduleDto>> GetAllAsync(int? classId = null, int? roomId = null, DateOnly? date = null)
    {
        var query = new List<string>();
        if (classId.HasValue) query.Add($"classId={classId}");
        if (roomId.HasValue) query.Add($"roomId={roomId}");
        if (date.HasValue) query.Add($"date={date.Value:yyyy-MM-dd}");
        var url = "/api/schedules" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<List<ScheduleDto>>(url) ?? new();
    }

    public async Task<List<TimeSlotDto>> GetTimeSlotsAsync() =>
        await _api.GetAsync<List<TimeSlotDto>>("/api/schedules/timeslots") ?? new();

    public async Task<bool> CreateAsync(CreateScheduleDto dto)
    {
        var res = await _api.PostRawAsync("/api/schedules", dto);
        return res.IsSuccessStatusCode;
    }
}
