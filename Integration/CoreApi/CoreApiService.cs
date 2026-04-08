using System.Net.Http.Json;

namespace DepartmentLoadApp.Integration.CoreApi;

public class CoreApiService
{
    private readonly HttpClient _http;

    public CoreApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<T>> GetListAsync<T>(string url)
    {
        var result = await _http.GetFromJsonAsync<List<T>>(url);
        return result ?? new List<T>();
    }
}