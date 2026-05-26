using CenterManagement.Models.DTOs;

namespace CenterManagement.WinForms.Services;

public interface IBranchService
{
    Task<List<BranchDto>> GetAllAsync();
    Task<List<RoomDto>> GetRoomsAsync(int branchId);
    Task<bool> CreateBranchAsync(CreateBranchDto dto);
    Task<bool> UpdateBranchAsync(int id, UpdateBranchDto dto);
    Task<List<RoomDto>> GetAllRoomsAsync(int? branchId = null);
    Task<bool> CreateRoomAsync(CreateRoomDto dto);
    Task<bool> UpdateRoomAsync(int id, UpdateRoomDto dto);
}

public class BranchService : IBranchService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<BranchDto>> GetAllAsync() =>
        await _api.GetAsync<List<BranchDto>>("/api/branches") ?? new();

    public async Task<List<RoomDto>> GetRoomsAsync(int branchId) =>
        await _api.GetAsync<List<RoomDto>>($"/api/branches/{branchId}/rooms") ?? new();

    public async Task<bool> CreateBranchAsync(CreateBranchDto dto)
    {
        var res = await _api.PostRawAsync("/api/branches", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateBranchAsync(int id, UpdateBranchDto dto)
    {
        var res = await _api.PutRawAsync($"/api/branches/{id}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<RoomDto>> GetAllRoomsAsync(int? branchId = null)
    {
        var url = branchId.HasValue ? $"/api/rooms?branchId={branchId}" : "/api/rooms";
        return await _api.GetAsync<List<RoomDto>>(url) ?? new();
    }

    public async Task<bool> CreateRoomAsync(CreateRoomDto dto)
    {
        var res = await _api.PostRawAsync("/api/rooms", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateRoomAsync(int id, UpdateRoomDto dto)
    {
        var res = await _api.PutRawAsync($"/api/rooms/{id}", dto);
        return res.IsSuccessStatusCode;
    }
}
