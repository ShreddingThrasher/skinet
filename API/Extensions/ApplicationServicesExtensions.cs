using API.Errors;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Api.Extensions
{
	public static class ApplicationServicesExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
		{
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();

			services.AddDbContext<StoreContext>(options =>
			{
				options.UseSqlite(config.GetConnectionString("DefaultConnection"));
			});
			
			services.AddSingleton<IConnectionMultiplexer>(c => 
			{
				var options = ConfigurationOptions.Parse(
					config.GetConnectionString("Redis")
				);
				
				options.AbortOnConnectFail = false;
				
				return ConnectionMultiplexer.Connect(options);
			});
			services.AddScoped<IBasketRepository, BasketRepository>();
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = actionContext =>
				{
					var errors = actionContext.ModelState
						.Where(e => e.Value.Errors.Count > 0)
						.SelectMany(x => x.Value.Errors)
						.Select(x => x.ErrorMessage)
						.ToArray();

					var errorResponse = new ApiValidationErrorResponse
					{
						Errors = errors
					};

					return new BadRequestObjectResult(errorResponse);
				};
			});
			
			services.AddCors(options => 
			{
				options.AddPolicy("CorsPolicy", policy => 
				{
					policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
				});
			});
			
			return services;
		}

		public static async Task<WebApplication> MigrateDatabase(this WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var services = scope.ServiceProvider;
			var context = services.GetRequiredService<StoreContext>();
			var logger = services.GetRequiredService<ILogger<Program>>();

			try
			{
				await context.Database.MigrateAsync();
				await StoreContextSeed.SeedAsync(context);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occured during migration");
			}

			return app;
		}
	}
}