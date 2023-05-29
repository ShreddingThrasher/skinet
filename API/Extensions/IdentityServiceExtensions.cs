using System.Text;
using Core.Entities.Identity;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
	public static class IdentityServiceExtensions
	{
		public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
		{
			services.AddDbContext<AppIdentityDbContext>(options => 
			{
				options.UseSqlite(config.GetConnectionString("IdentityConnection"));
			});
			
			services.AddIdentityCore<AppUser>(options => 
			{
				
			})
			.AddEntityFrameworkStores<AppIdentityDbContext>()
			.AddSignInManager<SignInManager<AppUser>>();
			
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options => 
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Token:Key"])),
						ValidIssuer = config["Token:Issuer"],
						ValidateAudience = false,
						ValidateIssuer = true
					};
				});
			
			services.AddAuthorization();
			
			return services;
		}
		
		public static async Task<WebApplication> MigradeIdentityDatabase(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var services = scope.ServiceProvider;
			var identityContext = services.GetRequiredService<AppIdentityDbContext>();
			var userManager = services.GetRequiredService<UserManager<AppUser>>();
			var logger = services.GetRequiredService<ILogger<Program>>();

			try
			{
				await identityContext.Database.MigrateAsync();
				await AppIdentityDbContextSeed.SeedUserAsync(userManager);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occured during migration");
			}

			return app;
		}
	}
}