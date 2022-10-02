using MyHabr.Entities;
using MyHabr.Enums;

namespace MyHabr.Models
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Login { get; set; }
        public UserState UserState { get; set; }
        public List<Role> Roles { get; set; } = new();
        public string Token { get; set; }

        public LoginResponse(User user, string token)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            Login = user.Login;
            UserState = user.UserState;
            Roles = user.Roles;
            Token = token;
        }
    }
}
