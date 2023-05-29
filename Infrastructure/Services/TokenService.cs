using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services
{
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _config;
		private readonly SymmetricSecurityKey _key;
		
		public TokenService(IConfiguration config)
		{
			this._config = config;
			this._key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(this._config["Token:Key"]));
		}
		
		public string CreateToken(AppUser user)
		{
			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.GivenName, user.DisplayName)
			};
			
			var credentials = new SigningCredentials(this._key, SecurityAlgorithms.HmacSha256Signature);
			
			var tokenDescriptor = new SecurityTokenDescriptor()
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.Now.AddDays(7),
				SigningCredentials = credentials,
				Issuer = this._config["Token:Issuer"]
			};
			
			var tokenHandler = new JwtSecurityTokenHandler();
			
			var token = tokenHandler.CreateToken(tokenDescriptor);
			
			return tokenHandler.WriteToken(token);
		}
	}
}