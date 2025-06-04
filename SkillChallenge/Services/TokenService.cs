using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace ASPNET_VisualStudio_Tutorial.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<User> _userManager;

        public TokenService(IConfiguration config, UserManager<User> userManager)
        {
            _config = config;
            _userManager = userManager;
            var signingKey = _config["JWT:SigningKey"];
            if (string.IsNullOrWhiteSpace(signingKey))
                throw new InvalidOperationException("JWT:SigningKey is not configured.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }

        public async Task<string> CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("userName", user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"],
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
