using Application.Common;
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

        public static DefaultUserClaims GetDefaultUserClaims(this HttpRequest req)
        {
            var claims = new DefaultUserClaims
            {
                Id = GetCurrentUserId(req),
                Name = GetCurrentUserName(req),
                Email = GetCurrentUserEmail(req),
                Role = GetCurrentUserRole(req),
            };

            if (string.IsNullOrEmpty(claims.Id) || string.IsNullOrEmpty(claims.Name) || string.IsNullOrEmpty(claims.Email) || string.IsNullOrEmpty(claims.Role))
            {
                throw new UnauthorizedException("Unauthorized");
            }

            return claims;
        }

        public static string GetCurrentUserId(this HttpRequest req)
        {
            return req.Headers["CurrentUserId"];
        }

        public static string GetCurrentUserName(this HttpRequest req)
        {
            return req.Headers["CurrentUserName"];
        }

        public static string GetCurrentUserEmail(this HttpRequest req)
        {
            return req.Headers["CurrentUserEmail"];
        }

        public static string GetCurrentUserRole(this HttpRequest req)
        {
            return req.Headers["CurrentUserRole"];
        }
    }
}
