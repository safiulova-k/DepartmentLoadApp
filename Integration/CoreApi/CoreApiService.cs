using System.Net.Http.Json;

namespace DepartmentLoadApp.Integration.CoreApi;

public class CoreApiService
{
    private readonly HttpClient _httpClient;

    public CoreApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<T>> GetListAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<T>>();
        return result ?? new List<T>();
    }
}