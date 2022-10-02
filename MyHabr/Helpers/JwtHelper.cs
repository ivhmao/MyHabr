using Microsoft.IdentityModel.Tokens;
using MyHabr.Entities;
using MyHabr.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyHabr.Helpers
{
    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _appConfig;

        public JwtHelper(IConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        public string GeterateJwtToken(User user)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.PrimarySid, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Version, Guid.NewGuid().ToString()));

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appConfig["SigningKey"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                    issuer: _appConfig["Issuer"],
                    audience: _appConfig["Audience"],
                    signingCredentials: signingCredentials,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Double.Parse(_appConfig["expiresMinutes"]))
                    );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int? VerifyJwtToken(string token)
        {
            if (token == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appConfig["SigningKey"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.PrimarySid).Value);

                //return user id from JWT token if validation successful
                return userId;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}
