using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetAllAsync(int? targetUserId = null);
    Task<bool> CreateAsync(CreateNotificationDto dto);
    Task<bool> MarkReadAsync(int id);
}

public class NotificationService : INotificationService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<NotificationDto>> GetAllAsync(int? targetUserId = null)
    {
        var url = "/api/notifications" + (targetUserId.HasValue ? $"?targetUserId={targetUserId}" : "");
        return await _api.GetAsync<List<NotificationDto>>(url) ?? new();
    }

    public async Task<bool> CreateAsync(CreateNotificationDto dto)
    {
        var res = await _api.PostRawAsync("/api/notifications", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> MarkReadAsync(int id)
    {
        var res = await _api.PostRawAsync($"/api/notifications/{id}/read", new { });
        return res.IsSuccessStatusCode;
    }
}
