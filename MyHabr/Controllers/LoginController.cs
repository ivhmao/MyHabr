using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyHabr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost, Route("login")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDTO.UserName) || string.IsNullOrEmpty(loginDTO.Password))
                        return BadRequest("Username and/or Password not specified");

                if (loginDTO.UserName.Equals("joydip") && loginDTO.Password.Equals("joydip123"))
                {
                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisisasecretkey@123"));
                    var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                    var jwtSecretToken = new JwtSecurityToken(
                        issuer: "ABCYZ",
                        audience: "http://localhost:7178",
                        signingCredentials: signingCredentials,
                        claims: new List<Claim>(),
                        expires: DateTime.Now.AddMinutes(10)
                        );
                    return Ok(new JwtSecurityTokenHandler().WriteToken(jwtSecretToken));
                }
            }
            catch
            {
                return BadRequest("An error ocured in generating the token");
            }
            return Unauthorized();
        }
    }
}
