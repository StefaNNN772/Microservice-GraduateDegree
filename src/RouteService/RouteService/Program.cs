using Microsoft.EntityFrameworkCore;
using RouteService.Clients.Interfaces;
using RouteService.Clients;
using RouteService.Data;
using RouteService.Helpers;
using RouteService.Repository;
using RouteService.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

//builder.Services.AddHttpClient<IAuthServiceClient, AuthServiceClient>();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        var secret = builder.Configuration["JWT:Secret"];
//        var issuer = builder.Configuration["JWT:Issuer"];
//        var audience = builder.Configuration["JWT:Audience"];

//        Console.WriteLine($"[ROUTE SERVICE] JWT Config:");
//        Console.WriteLine($"  Secret Length: {secret?.Length ?? 0}");
//        Console.WriteLine($"  Issuer: {issuer}");
//        Console.WriteLine($"  Audience: {audience}");

//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = issuer,
//            ValidAudience = audience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = context =>
//            {
//                Console.WriteLine($"[ROUTE SERVICE] Authentication FAILED!");
//                Console.WriteLine($"  Exception: {context.Exception.Message}");
//                return Task.CompletedTask;
//            },
//            OnTokenValidated = context =>
//            {
//                Console.WriteLine($"[ROUTE SERVICE] Token VALIDATED!");
//                return Task.CompletedTask;
//            }
//        };
//    });

//builder.Services.AddAuthorization();

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

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();