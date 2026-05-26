using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CenterManagement.WinForms.Core;

public class ApiClient
{
    public static ApiClient Instance { get; } = new();

    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5140";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private ApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<T?> PostAsync<T>(string endpoint, object body)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<HttpResponseMessage> PostRawAsync(string endpoint, object body)
    {
        return await _httpClient.PostAsJsonAsync(endpoint, body);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<HttpResponseMessage> GetRawAsync(string endpoint)
    {
        return await _httpClient.GetAsync(endpoint);
    }

    public async Task<HttpResponseMessage> PutRawAsync(string endpoint, object body)
    {
        return await _httpClient.PutAsJsonAsync(endpoint, body);
    }

    public async Task<HttpResponseMessage> DeleteRawAsync(string endpoint)
    {
        return await _httpClient.DeleteAsync(endpoint);
    }
}
