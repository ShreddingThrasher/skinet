using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity
{
	public class AppIdentityDbContextSeed
	{
		public static async Task SeedUserAsync(UserManager<AppUser> userManager)
		{
			if(!userManager.Users.Any())
			{
				var user = new AppUser()
				{
					DisplayName = "Bob",
					Email = "bob@test.com",
					UserName = "bob@test.com",
					Address = new Address()
					{
						FirstName = "Bob",
						LastName = "Bobbity",
						Street = "Elm Street",
						City = "New Your",
						State = "NY",
						ZipCode = "90123"
					}
				};
				
				await userManager.CreateAsync(user, "Pa$$w0rd");
			}
		}
	}
}