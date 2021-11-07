using Newtonsoft.Json;

namespace Application.Accounts.Queries.GetIdentity
{
    public class AccountDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public Role Role { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public EmployeeRole? EmployeeRole { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
