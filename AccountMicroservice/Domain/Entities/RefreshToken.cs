using Newtonsoft.Json;

namespace Domain.Entities
{
    public class RefreshToken
    {
        [JsonProperty("id")]
        public string Token { get; set; }
        public string UserId { get; set; }
        public int Ttl { get; set; } = 604800;
    }
}
