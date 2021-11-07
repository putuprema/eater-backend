using Domain.Constants;

namespace Domain.Entities
{
    public class Account
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public Role Role { get; set; }
        public EmployeeRole? EmployeeRole { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
