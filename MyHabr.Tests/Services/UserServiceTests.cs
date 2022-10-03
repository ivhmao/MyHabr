using Xunit;
using MyHabr.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHabr.Helpers;
using MyHabr.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Moq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyHabr.Models;
using NuGet.Protocol.Plugins;
using MyHabr.Entities;
using MyHabr.Enums;
using NuGet.Common;
using System.Xml.Linq;
using System.Data.Entity;

namespace MyHabr.Services.Tests
{
    public class UserServiceTests
    {
        private readonly UserService _userService;
        private readonly IJwtHelper _jwtHelper;

        public UserServiceTests()
        {
    //        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    //        optionsBuilder.UseSqlite("Filename=test.db");

    //        var dbOption = new DbContextOptionsBuilder<AppDbContext>()
    //.UseSqlServer("Filename=test.db")
    //.Options;

    //        var appDbContext = new AppDbContext(dbOption);


            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseInMemoryDatabase("MyInMemoryDatabseName");
            var appDbContext = new AppDbContext(optionsBuilder.Options);


            //Arrange
            var inMemorySettings = new Dictionary<string, string>
            { 
                {"Issuer", "ABCYZ" },
                {"Audience","http://localhost:7178" },
                {"SigningKey", "thisisasecretkey@123"},
                {"expiresMinutes", "30" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _jwtHelper = new JwtHelper(configuration);
            _userService = new UserService(appDbContext, _jwtHelper);
        }

        [Fact()]
        public void UserServiceTest()
        {
            //Arrange
            Type t = typeof(UserService);
            FieldInfo f1 = t.GetField("_appDbContext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            FieldInfo f2 = t.GetField("_jwtHelper", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            //Act
            var checkedAppDbContext = f1.GetValue(_userService);
            var checkedJwtHelper = f2.GetValue(_userService);

            //Assert
            Assert.NotNull(checkedAppDbContext);
            Assert.NotNull(checkedJwtHelper);
        }

        [Fact()]
        public void LoginTest()
        {
            //Arrange
            LoginRequest loginRequest = new LoginRequest { UserName="admin", Password="admin" };
            //Act
            var loginResponce = _userService.Login(loginRequest);
            var _token = loginResponce.Token;
            //Assert
            Assert.NotNull(loginResponce);
            Assert.NotNull(_token);
            Assert.Equal(1, loginResponce.Id);
            Assert.Equal("admin", loginResponce.Login);
        }

        [Fact()]
        public void RegistrationTest()
        {
            //Arrange
            var userDTO = new UserDTO { Login = "testeduser", 
                                        Password = "gfdblkjfdlk23#E#gfd",
                                        Email = "test@test.com",
                                        Name = "TestedUser"
            };
            
            //Act
            var loginResponce = _userService.Registration(userDTO);
            var userIdFromToken = _jwtHelper.VerifyJwtToken(loginResponce.Token);
            //Assert
            Assert.NotNull(loginResponce);
            Assert.True(loginResponce.UserState == Enums.UserState.NotVerified);
            Assert.True(loginResponce.Email == userDTO.Email);
            Assert.True(loginResponce.Name == userDTO.Name);
            Assert.True(loginResponce.Login == userDTO.Login);
            Assert.True(loginResponce.Id>0);
            Assert.True(userIdFromToken == loginResponce.Id);
            Assert.Collection(loginResponce.Roles, item => Assert.Contains("Reader",item.Name));

            _userService.DeleteById(loginResponce.Id);
        }

        [Fact()]
        public void VerifyTest()
        {
            //Arrange
            var userDTO = new UserDTO
            {
                Login = "testeduser2",
                Password = "gfdblkjfdlk23#E#gfd",
                Email = "test2@test.com",
                Name = "TestedUser2"
            };
            //Act
            var loginResponce = _userService.Registration(userDTO);
            var userIdFromToken = _jwtHelper.VerifyJwtToken(loginResponce.Token);

            var user = _userService.GetById(loginResponce.Id);
            var varificationResponce = _userService.Verify(user.VerificationCode);
            //Assert
            Assert.True(varificationResponce);

            _userService.DeleteById(loginResponce.Id);
        }

        [Fact()]
        public void GetAllTest()
        {
            //Arrange

            //Act
            var allUsers = _userService.GetAll();

            //Assert
            Assert.NotEmpty(allUsers);
            Assert.IsType<List<User>>(allUsers);
            Assert.True(allUsers.Count()>=1);
        }

        [Fact()]
        public void GetByIdTest()
        {
            //Arrange

            //Act
            var user = _userService.GetById(1);

            //Assert
            Assert.NotNull(user);
            Assert.IsType<User>(user);
            Assert.Equal("admin", user.Login);
        }

        [Fact()]
        public void EditTest()
        {
            //Arrange
            var userAdmin = _userService.GetById(1);
            userAdmin.Name = "EditedAdmin";
            UserDTO userDTO = new UserDTO();
            userDTO.Id = userAdmin.Id;
            userDTO.Login = userAdmin.Login;
            userDTO.Name = "EditedAdmin";
            userDTO.Email = "Edited@email.com";

            //Act
            var editedUser = _userService.Edit(userDTO);

            //Assert
            Assert.Equal("EditedAdmin", editedUser.Name);
            Assert.Equal("Edited@email.com", editedUser.Email);
            Assert.Equal(userAdmin.Id, editedUser.Id);
            Assert.Equal(userAdmin.Login, editedUser.Login);
        }

        [Fact()]
        public void SetRolesTest()
        {
            //Arrange
            var userDTO = new UserDTO
            {
                Login = "testeduser2",
                Password = "gfdblkjfdlk23#E#gfd",
                Email = "test2@test.com",
                Name = "TestedUser2"
            };

            var loginResponce = _userService.Registration(userDTO);
            var userIdFromToken = _jwtHelper.VerifyJwtToken(loginResponce.Token);

            var user = _userService.GetById(loginResponce.Id);
            var varificationResponce = _userService.Verify(user.VerificationCode);

            RoleDTO writerRole = new RoleDTO { Name = "Writer" };
            RoleDTO moderatorRole = new RoleDTO { Name = "Moderator" };
            var listRoles = new List<RoleDTO>();
            listRoles.Add(writerRole);
            listRoles.Add(moderatorRole);

            //Act
            var editedUser = _userService.SetRoles(user.Id, listRoles);

            //Assert
            Assert.True(user.Id==editedUser.Id);
            Assert.Collection(editedUser.Roles, item => Assert.Contains("Reader", item.Name),
                                                item => Assert.Contains("Writer", item.Name),
                                                item => Assert.Contains("Moderator", item.Name));

        }

        [Fact()]
        public void UnsetRolesTest()
        {
            //Arrange
            var userDTO = new UserDTO
            {
                Login = "testeduser4",
                Password = "gfdblkjfdlk23#E#gfd",
                Email = "test4@test.com",
                Name = "TestedUser4"
            };

            var loginResponce = _userService.Registration(userDTO);
            var userIdFromToken = _jwtHelper.VerifyJwtToken(loginResponce.Token);

            var user = _userService.GetById(loginResponce.Id);
            var varificationResponce = _userService.Verify(user.VerificationCode);

            RoleDTO writerRole = new RoleDTO { Name = "Writer" };
            RoleDTO moderatorRole = new RoleDTO { Name = "Moderator" };
            var listRoles = new List<RoleDTO>();
            listRoles.Add(writerRole);
            listRoles.Add(moderatorRole);
            var editedUser = _userService.SetRoles(user.Id, listRoles);

            var unsetedListRoles = editedUser.Roles;
            unsetedListRoles.Remove(unsetedListRoles.First());

            //Act
            var editedUser2 = _userService.SetRoles(editedUser.Id, listRoles);

            //Assert
            Assert.True(user.Id == editedUser.Id);
            Assert.Collection(editedUser2.Roles, item => Assert.Contains("Writer", item.Name),
                                                 item => Assert.Contains("Moderator", item.Name));
        }

        [Fact()]
        public void DeleteByIdTest()
        {
            //Arrange
            var userDTO = new UserDTO
            {
                Login = "testeduser5",
                Password = "gfdblkjfdlk23#E#gfd",
                Email = "test5@test.com",
                Name = "TestedUser5"
            };
            var loginResponce = _userService.Registration(userDTO);
            var user = _userService.GetById(loginResponce.Id);

            Assert.NotNull(user);

            //Act
            var boolVar = _userService.DeleteById(user.Id);

            //Assert
            Assert.True(boolVar);
        }

        [Fact()]
        public void ChangePasswordTest()
        {
            //Arrange
            var userAdmin = _userService.GetById(1);

            //Act
            var boolVar = _userService.ChangePassword(userAdmin, "admin", "adminadmin");

            //Assert
            Assert.True(boolVar);
            Assert.True(_userService.ChangePassword(userAdmin, "adminadmin", "admin"));
        }

        [Fact()]
        public void GetCurrentUserTest()
        {
            //Arrange
            LoginRequest loginRequest = new LoginRequest { UserName = "admin", Password = "admin" };
            //Act
            var loginResponce = _userService.Login(loginRequest);
            var token = loginResponce.Token;

            //Act
            var currUser = _userService.GetCurrentUser(token);

            //Assert
            Assert.NotNull(currUser);
            Assert.True(loginResponce.Id == currUser.Id);
        }
    }
}