using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHabr.Entities;
using MyHabr.Helpers;
using MyHabr.Interfaces;
using MyHabr.Models;

namespace MyHabr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        #region non-standart methods (login, registration, verification, changepass)

        [HttpPost, AllowAnonymous, Route("Login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            try
            {
                return Ok(_userService.Login(loginRequest));
            }
            catch
            {
                return BadRequest("An error ocured in generating the token");
            }
            return Unauthorized();
        }

        [HttpPost, AllowAnonymous, Route("Registration")]
        public IActionResult Registration(UserDTO userDTO)
        {
            try
            {
                return Ok(_userService.Registration(userDTO));
            }
            catch
            {
                return BadRequest("An error ocured in generating the token");
            }
            return Unauthorized();
        }

        [HttpPost, AllowAnonymous, Route("Verify")]
        public IActionResult Verify(string verificationCode)
        {
            if (!_userService.Verify(verificationCode))
                return BadRequest("Verification error");
            return Ok("User verified");
        }

        [Authorize]
        [HttpPost, Route("ChangePassword")]
        public ActionResult<bool> ChangePassword(ChangePasswordRequest request)
        {
            var _context = ControllerContext.HttpContext;
            var token = _context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null) return NotFound();

            var currentUser = _userService.GetCurrentUser(token);
            if (currentUser == null) return NotFound();

            _userService.ChangePassword(currentUser, request.OldPassword, request.NewPassword);

            return NoContent();
        }
        #endregion

        #region User CRUD

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            return _userService.GetAll();
        }

        // GET: api/Users/5
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return user;
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult<User> UpdateUser(int id, UserDTO userDTO)
        {
            if (id != userDTO.Id)
            {
                return BadRequest();
            }

            var _context = ControllerContext.HttpContext;
            var token = _context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null) return BadRequest("Need authentication");

            var currentUser = _userService.GetCurrentUser(token);

            if (currentUser == null)
            {
                return BadRequest("Need authentication");
            }
            
            if (!currentUser.Roles.Any(u => u.Name.Equals("Admin")) && id!=currentUser.Id)
            {
                return BadRequest("Only admin can edit users");
            }

            var user = _userService.Edit(userDTO);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int id)
        {
            if (_userService.DeleteById(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("UnsetRoles/{id}")]
        public ActionResult<User> UnsetRoles(int id, IList<RoleDTO> listRoles)
        {
            var user = _userService.UnsetRoles(id, listRoles);

            if (user == null) return NotFound("User not found");

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("SetRoles/{id}")]
        public ActionResult<User> SetRoles(int id, IList<RoleDTO> listRoles)
        {
            var user = _userService.SetRoles(id, listRoles);

            if (user == null) return NotFound("User not found");

            return Ok(user);
        }

        #endregion
    }
}
