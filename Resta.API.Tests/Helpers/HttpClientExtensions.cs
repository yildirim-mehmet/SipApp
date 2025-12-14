using System.Net.Http.Json;

namespace Resta.API.Tests.Helpers;

public static class HttpClientExtensions
{
    // ✅ JSON dönen GET'ler için
    public static async Task<T> GetJsonAsync<T>(
        this HttpClient client,
        string url)
    {
        var res = await client.GetAsync(url);
        res.EnsureSuccessStatusCode();

        return await res.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("Response body boş.");
    }

    // ✅ Body dönmeyen GET'ler için
    public static async Task GetNoContentAsync(
        this HttpClient client,
        string url)
    {
        var res = await client.GetAsync(url);
        res.EnsureSuccessStatusCode();
    }

    // ✅ POST JSON
    public static async Task<T> PostAsync<T>(
        this HttpClient client,
        string url,
        object body)
    {
        var res = await client.PostAsJsonAsync(url, body);
        res.EnsureSuccessStatusCode();

        return await res.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("Response body boş.");
    }
}
