using MyHabr.Entities;
using MyHabr.Models;

namespace MyHabr.Interfaces
{
    public interface IUserService
    {
        bool ChangePassword(User user, string oldPassword, string newPassword);
        bool DeleteById(int id);
        User? Edit(UserDTO userDTO);
        IEnumerable<User> GetAll();
        User GetById(int id);
        LoginResponse Login(LoginRequest loginRequest);
        LoginResponse Registration(UserDTO userDTO);
        bool Verify(string verificationCode);
        User GetCurrentUser(string token);
        User? SetRoles(int id, IList<RoleDTO> listRoles);
        User? UnsetRoles(int id, IList<RoleDTO> listRoles);
    }
}