using Microsoft.EntityFrameworkCore;
using RouteService.Clients.Interfaces;
using RouteService.Clients;
using RouteService.Data;
using RouteService.Helpers;
using RouteService.Repository;
using RouteService.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BusLinesRepository>();
builder.Services.AddScoped<SchedulesRepository>();
builder.Services.AddScoped<FavouritesRepository>();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<MapAPI>();
builder.Services.AddScoped<BusLinesService>();
builder.Services.AddScoped<SchedulesService>();
builder.Services.AddScoped<FavouritesService>();

builder.Services.AddHttpClient<IAuthServiceClient, AuthServiceClient>();
builder.Services.AddHttpClient<ITicketServiceClient, TicketServiceClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>())
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("RouteService: Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"RouteService: Database migration failed: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseMetricServer();
app.UseHttpMetrics();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();