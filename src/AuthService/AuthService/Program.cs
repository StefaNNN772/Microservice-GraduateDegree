using AuthService.Data;
using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repository;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProviderRepository>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ProviderService>();
builder.Services.AddScoped<AuthenticationManager>();

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
        Console.WriteLine("Database migration completed successfully.");

        if (!context.Users.Any(u => u.Email == "slazarevic772@gmail.com"))
        {
            var adminUser = new User
            {
                Name = "Stefan",
                LastName = "Lazarevic",
                Email = "slazarevic772@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("kalabunga"),
                Birthday = new DateTime(2002, 03, 21),
                Role = UserRole.Admin,
                DiscountType = null,
                DiscountStatus = DiscountStatus.NoRequest,
                DiscountValidUntil = DateTime.MinValue,
                DiscountDocumentPath = null,
                ProfileImagePath = null
            };

            context.Users.Add(adminUser);
            context.SaveChanges();

            Console.WriteLine("Admin user created successfully!");
            Console.WriteLine("Email: admin@serbijabus.com");
            Console.WriteLine("Password: admin123");
        }
        else
        {
            Console.WriteLine("Admin user already exists.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();