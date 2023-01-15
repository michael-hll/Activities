using Persistence;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
using API.Middleware;
//using Microsoft.EntityFrameworkCore;
// create Kestrel Server as the host
// and read appsettings.json & appsettings.Development.json
var builder = WebApplication.CreateBuilder(args);

/*...............................*/
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
var app = builder.Build();

/*.................................*/
// Configure the HTTP request pipeline.
// we add middleware in this section
app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch(Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration.");
}

app.Run();
