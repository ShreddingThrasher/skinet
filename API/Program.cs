using Api.Extensions;
using API.Middleware;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

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
app.UseAuthorization();

app.MapControllers();

await app.MigrateDatabase();

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
