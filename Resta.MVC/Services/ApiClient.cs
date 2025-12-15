using System.Net.Http.Json;

namespace Resta.MVC.Services
{
    // MVC → API iletişimini merkezi hale getirir
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http)
        {
            _http = http;
        }

        // GET işlemleri
        public async Task<T?> GetAsync<T>(string url)
        {
            return await _http.GetFromJsonAsync<T>(url);
        }

        // POST işlemleri
        public async Task<T?> PostAsync<T>(string url, object body)
        {
            var res = await _http.PostAsJsonAsync(url, body);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<T>();
        }
    }
}
