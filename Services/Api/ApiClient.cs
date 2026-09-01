using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinalProject.Services.Api;

public class ApiClient
{
    public static ApiClient Instance { get; } = new();

    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    public string BaseUrl
    {
        get => _http.BaseAddress?.ToString() ?? "";
        set => _http.BaseAddress = new Uri(value.EndsWith('/') ? value : value + "/");
    }

    public string? AuthToken { get; private set; }

    public ApiClient()
    {
        _http = new HttpClient();
        
        // Auto-detect Android Emulator vs Windows Machine
        var host = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
        _http.BaseAddress = new Uri($"http://{host}:8000/api/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void SetToken(string? token)
    {
        AuthToken = token;
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var res = await _http.GetAsync(endpoint);
            if (!res.IsSuccessStatusCode) return default;
            return await res.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient GET Error] {endpoint}: {ex.Message}");
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var res = await _http.PostAsJsonAsync(endpoint, data, JsonOptions);
            if (!res.IsSuccessStatusCode) return default;
            return await res.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient POST Error] {endpoint}: {ex.Message}");
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var res = await _http.DeleteAsync(endpoint);
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient DELETE Error] {endpoint}: {ex.Message}");
            return false;
        }
    }
}
