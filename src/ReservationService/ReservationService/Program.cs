using Microsoft.EntityFrameworkCore;
using ReservationService.Clients.Interfaces;
using ReservationService.Clients;
using ReservationService.Data;
using ReservationService.Helpers;
using ReservationService.Repository;
using ReservationService.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<BusReservationService>();
builder.Services.AddScoped<BusReservationRepository>();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddHttpClient<IAuthServiceClient, AuthServiceClient>();
builder.Services.AddHttpClient<IRouteServiceClient, RouteServiceClient>();

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
        Console.WriteLine("ReservationService: Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ReservationService: Database migration failed: {ex.Message}");
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