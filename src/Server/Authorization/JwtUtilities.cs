using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FootyTipping.Server.Entitites;
using FootyTipping.Server.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace FootyTipping.Server.Authorization
{
    public interface IJwtUtilities
    {
        public string GenerateToken(User user);
        public int? ValidateToken(string token);
    }

    public class JwtUtilities : IJwtUtilities
    {
        private readonly SecuritySettings _securitySettings;

        public JwtUtilities(IOptions<SecuritySettings> securitySettings)
        {
            _securitySettings = securitySettings.Value;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_securitySettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[] {new Claim("id", user.Id.ToString())}),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public int? ValidateToken(string token)
        {
            if (token == null)
                return null;


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_securitySettings.Secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // so that tokens expire exactly at expiration (not 5 minutes after)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                return userId;
            }
            catch
            {
                // just return null if validation fails
                return null;
            }
        }
    }
}
