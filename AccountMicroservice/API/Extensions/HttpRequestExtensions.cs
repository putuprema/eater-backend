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

        public static Role GetCurrentUserRole(this HttpRequest req)
        {
            if (req.Headers.TryGetValue("CurrentUserRole", out var roleHeaderValue))
            {
                var roleHeaderSplit = roleHeaderValue.First().Split("_");
                if (Enum.TryParse(roleHeaderSplit[0], out Role role))
                {
                    return role;
                }
            }

            return Role.None;
        }
    }
}
