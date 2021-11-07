using Newtonsoft.Json;

namespace API.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> ReadFromJsonAsync<T>(this HttpRequest req)
        {
            string requestBody = await req.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }

        public static string GetCurrentUserId(this HttpRequest req)
        {
            return req.Headers["CurrentUserId"];
        }
    }
}
