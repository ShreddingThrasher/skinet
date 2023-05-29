using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.MigrateDatabase();
await app.MigradeIdentityDatabase();

// using var scope = app.Services.CreateScope();
// var services = scope.ServiceProvider;
// var context = services.GetRequiredService<StoreContext>();
// var logger = services.GetRequiredService<ILogger<Program>>();

// try
// {
//     await context.Database.MigrateAsync();
//     await StoreContextSeed.SeedAsync(context);
// }
// catch (Exception ex)
// {
//     logger.LogError(ex, "An error occured during migration");
// }

app.Run();
