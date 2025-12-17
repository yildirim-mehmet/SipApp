using System.Net.Http.Json;
using System.Text.Json;

namespace Resta.MVC.Services;

/// <summary>
/// MVC -> API çağrılarını tek yerden yönetir.
/// BaseAddress Program.cs içinde appsettings.json'dan set edilir.
/// 
/// Önemli: API bazı endpoint'lerde null/boş body döndürebilir.
/// Bu yüzden ReadFromJsonAsync öncesi body var mı kontrol ediyoruz.
/// </summary>
/// 

public class ApiClient
{
    private readonly HttpClient _http;

    // 🔴 BUNU EKLE
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
    }


    public async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken ct = default)
    {
        var res = await _http.GetAsync(relativeUrl, ct);
        var raw = await res.Content.ReadAsStringAsync(ct);

        //// 🔴 GEÇİCİ DEBUG
        //if (!raw.TrimStart().StartsWith("{"))
        //    throw new Exception($"API JSON dönmedi. URL: {relativeUrl}\nResponse:\n{raw}");
        var trimmed = raw.TrimStart();
        if (!(trimmed.StartsWith("{") || trimmed.StartsWith("[")))
        {
            throw new Exception(
                $"API JSON dönmedi. URL: {relativeUrl}\nResponse:\n{raw}"
            );
        }


        return JsonSerializer.Deserialize<T>(raw, JsonOpts);
    }


    //public async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken ct = default)
    //{
    //    var res = await _http.GetAsync(relativeUrl, ct);
    //    if (!res.IsSuccessStatusCode) return default;

    //    if (res.Content.Headers.ContentLength == 0) return default;

    //    // 🔴 JsonOpts ile deserialize et
    //    return await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
    //}

    public async Task<T?> PostAsync<T>(string relativeUrl, object body, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync(relativeUrl, body, ct);
        if (!res.IsSuccessStatusCode) return default;

        if (res.Content.Headers.ContentLength == 0) return default;

        return await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
    }
}




//public class ApiClient
//{
//    private readonly HttpClient _http;

//    // API tarafı camelCase döndürüyor olabilir; tolerant olalım.
//    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
//    {
//        PropertyNameCaseInsensitive = true
//    };

//    public ApiClient(HttpClient http)
//    {
//        _http = http;
//    }

//    public async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken ct = default)
//    {
//        var res = await _http.GetAsync(relativeUrl, ct);
//        if (!res.IsSuccessStatusCode) return default;

//        // body boş olabilir
//        if (res.Content.Headers.ContentLength == 0) return default;

//        return await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
//    }

//    public async Task<T?> PostAsync<T>(string relativeUrl, object body, CancellationToken ct = default)
//    {
//        var res = await _http.PostAsJsonAsync(relativeUrl, body, ct);
//        if (!res.IsSuccessStatusCode) return default;

//        if (res.Content.Headers.ContentLength == 0) return default;

//        return await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
//    }
//}
