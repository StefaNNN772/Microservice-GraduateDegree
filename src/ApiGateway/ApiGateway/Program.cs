using ApiGateway.Middleware;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SrbijaBus API Gateway",
        Version = "v1",
        Description = "API Gateway for SrbijaBus microservices - Bus reservation system",
        Contact = new OpenApiContact
        {
            Name = "StefaNNN772",
            Email = "stefan@srbijabus.com",
            Url = new Uri("https://github.com/StefaNNN772/Microservice-GraduateDegree")
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme. Enter 'Bearer' [space] and then your token from /login endpoint",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<ProxyService>();
builder.Services.AddScoped<ServiceDiscoveryService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SrbijaBusPolicy", policy =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["JwtSettings:Key"] ??
                     Environment.GetEnvironmentVariable("JWT_SECRET") ??
                     "your_super_secret_jwt_key_here_minimum_32_characters_for_production";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("SrbijaBusApi", configure =>
    {
        configure.PermitLimit = 100;
        configure.Window = TimeSpan.FromMinutes(1);
        configure.QueueLimit = 20;
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("gateway-self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SrbijaBus API Gateway v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "SrbijaBus API Documentation";
    });
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseHttpMetrics();

app.UseCors("AllowAngularApp");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<JwtValidationMiddleware>();

app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health");

app.Map("/login", loginApp =>
{
    loginApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "UserService", "/login");
    });
});

app.Map("/register", registerApp =>
{
    registerApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "UserService", "/register");
    });
});

app.Map("/users", usersApp =>
{
    usersApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "UserService", "/users");
    });
});

app.Map("/providers", providersApp =>
{
    providersApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "UserService", "/providers");
    });
});

app.Map("/schedules", schedulesApp =>
{
    schedulesApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "RouteService", "/schedules");
    });
});

app.Map("/busLines", busLinesApp =>
{
    busLinesApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "RouteService", "/busLines");
    });
});

app.Map("/favourites", favouritesApp =>
{
    favouritesApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "RouteService", "/favourites");
    });
});

app.Map("/busReservation", busReservationApp =>
{
    busReservationApp.Run(async context =>
    {
        var proxyService = context.RequestServices.GetRequiredService<ProxyService>();
        await proxyService.ProxyRequestAsync(context, "ReservationService", "/busReservation");
    });
});

Console.WriteLine("=== SrbijaBus API Gateway Started ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Swagger UI: http://localhost:8080");
Console.WriteLine($"Health Check: http://localhost:8080/health");
Console.WriteLine($"Metrics: http://localhost:8080/metrics");
Console.WriteLine("======================================");

app.Run();