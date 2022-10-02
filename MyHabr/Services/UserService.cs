using MyHabr.Helpers;
using MyHabr.Models;
using Microsoft.EntityFrameworkCore;
using MyHabr.Interfaces;
using MyHabr.Entities;
using MyHabr.Enums;

namespace MyHabr.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IJwtHelper _jwtHelper;

        public UserService(AppDbContext appDbContext, IJwtHelper jwtHelper)
        {
            _appDbContext = appDbContext;
            _jwtHelper = jwtHelper;
        }

        public LoginResponse Login(LoginRequest loginRequest)
        {
            var user = _appDbContext.Users/*.Include(u=>u.Roles)*/.SingleOrDefault(u => u.Login.ToLower().Equals(loginRequest.UserName.ToLower()));

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                throw new Exception("User/password is incorrect");
            }

            _appDbContext.Entry(user).Collection(u => u.Roles).Load();
            //_appDbContext.Roles.Where(u => u.Users.Contains(user)).Load();

            var token = _jwtHelper.GeterateJwtToken(user);

            return new LoginResponse(user, token);
        }

        public LoginResponse Registration(UserDTO userDTO)
        {
            var user = _appDbContext.Users.SingleOrDefault(u => u.Login.ToLower().Equals(userDTO.Login.ToLower()));
            if (user != null)
            {
                throw new Exception("Cannot register new user. User already exists.");
            }

            var rolesForNewUser = new List<Role>();
            rolesForNewUser.Add(RoleEnum.Reader);

            var newUser = new User()
            {
                Login = userDTO.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                UserState = Enums.UserState.NotVerified,
                VerificationCode = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Email = userDTO.Email,
                Name = userDTO.Name,
                Roles = rolesForNewUser
            };

            _appDbContext.Users.Add(newUser);
            _appDbContext.SaveChanges();

            var token = _jwtHelper.GeterateJwtToken(newUser);

            return new LoginResponse(newUser, token);
        }

        public bool Verify(string verificationCode)
        {
            if (verificationCode == null) throw new Exception("Nothing to verify");

            var user = _appDbContext.Users.SingleOrDefault(u => u.VerificationCode == verificationCode);

            if (user == null) throw new Exception("Cannot verify user");
            if (user.UserState == Enums.UserState.Inactive) throw new Exception("Cannot verify. User banned.");
            if (user.UserState == Enums.UserState.Active) throw new Exception("User already verified.");

            user.UserState = Enums.UserState.Active;
            user.VerificationCode = null;

            _appDbContext.SaveChanges();

            return true;
        }

        public IEnumerable<User> GetAll()
        {
            return _appDbContext.Users.Include(u => u.Roles).ToList();
        }

        public User GetById(int id)
        {
            return _appDbContext.Users.Find(id);
        }

        public User? Edit(UserDTO userDTO)
        {
            if (!UserExists(userDTO.Id)) return null;

            var user = _appDbContext.Users.Find(userDTO.Id);
            if (user==null) return null;

            user.Name = userDTO.Name;
            user.Email = userDTO.Email;
            user.Login = userDTO.Login;

            _appDbContext.Entry(user).State = EntityState.Modified;
            _appDbContext.SaveChanges();

            return user;
        }

        public User? SetRoles(int id, IList<RoleDTO> listRoles)
        {
            if (!UserExists(id)) return null;

            var user = _appDbContext.Users.Find(id);
            if (user == null) return null;

            foreach (var r in listRoles)
            {
                if (!user.Roles.Any(_ => _.Name.Equals(r.Name)))
                {
                    var role = _appDbContext.Roles.FirstOrDefault(x => x.Name.Equals(r.Name));
                    if (role == null) continue;
                    user.Roles.Add(role);
                    _appDbContext.Entry(user).State = EntityState.Modified;
                }
                    
            }

            _appDbContext.SaveChanges();

            return user;
        }

        public User? UnsetRoles(int id, IList<RoleDTO> listRoles)
        {
            if (!UserExists(id)) return null;

            var user = _appDbContext.Users.Find(id);
            if (user == null) return null;

            foreach (var role in listRoles)
            {
                if (user.Roles.Any(_ => _.Name.Equals(role.Name)))
                {
                    user.Roles.Remove(new Role(role.Name));
                    _appDbContext.Entry(user).State = EntityState.Modified;
                }
                    
            }

            _appDbContext.SaveChanges();

            return user;
        }


        public bool DeleteById(int id)
        {
            var user = _appDbContext.Users.Find(id);
            if (user == null)
            {
                return false;
            }

            _appDbContext.Users.Remove(user);
            _appDbContext.SaveChanges();

            return true;
        }

        public bool ChangePassword(User user, string oldPassword, string newPassword)
        {
            if (user == null || !UserExists(user.Id)) return false;

            if (BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _appDbContext.Entry(user).State = EntityState.Modified;
                _appDbContext.SaveChanges();
            }
            else return false;

            return true;
        }

        public User GetCurrentUser(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = _jwtHelper.VerifyJwtToken(token);
            if (userId != null)
            {
                return GetById(userId.Value);
            }
            return null;
        }
        private bool UserExists(int id)
        {
            return _appDbContext.Users.Any(e => e.Id == id);
        }



    }
}
