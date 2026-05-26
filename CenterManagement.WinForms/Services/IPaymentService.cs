using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;

namespace CenterManagement.WinForms.Services;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetAllAsync(int? enrollmentId = null, PaymentStatus? status = null);
    Task<PaymentSummaryDto?> GetSummaryAsync(int? month = null, int? year = null);
    Task<(bool success, string message)> CreatePaymentAsync(CreatePaymentDto dto);
}

public class PaymentService : IPaymentService
{
    private readonly Core.ApiClient _api = Core.ApiClient.Instance;

    public async Task<List<PaymentDto>> GetAllAsync(int? enrollmentId = null, PaymentStatus? status = null)
    {
        var query = new List<string>();
        if (enrollmentId.HasValue) query.Add($"enrollmentId={enrollmentId}");
        if (status.HasValue) query.Add($"status={(int)status}");
        var url = "/api/payments" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<List<PaymentDto>>(url) ?? new();
    }

    public async Task<PaymentSummaryDto?> GetSummaryAsync(int? month = null, int? year = null)
    {
        var query = new List<string>();
        if (month.HasValue) query.Add($"month={month}");
        if (year.HasValue) query.Add($"year={year}");
        var url = "/api/payments/summary" + (query.Any() ? "?" + string.Join("&", query) : "");
        return await _api.GetAsync<PaymentSummaryDto>(url);
    }

    public async Task<(bool success, string message)> CreatePaymentAsync(CreatePaymentDto dto)
    {
        var res = await _api.PostRawAsync("/api/payments", dto);
        var msg = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, msg);
    }
}
