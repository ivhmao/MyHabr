using MyHabr.Enums;

namespace MyHabr.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Login { get; set; } = String.Empty;
        public string? PasswordHash { get; set; }
        public UserState UserState { get; set; }
        public string? VerificationCode { get; set; }
        public List<Role> Roles { get; set; } = new();
    }
}
