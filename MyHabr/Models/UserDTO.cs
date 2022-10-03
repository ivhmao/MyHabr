namespace MyHabr.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Login { get; set; } = String.Empty;
        public string? Password { get; set; }
    }
}
