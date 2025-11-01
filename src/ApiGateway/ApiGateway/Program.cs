using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.SetMinimumLevel(LogLevel.Debug);

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer("Bearer", options =>
//    {
//        var secret = builder.Configuration["JWT:Secret"];
//        var issuer = builder.Configuration["JWT:Issuer"];
//        var audience = builder.Configuration["JWT:Audience"];

//        Console.WriteLine($"[API GATEWAY] JWT Config:");
//        Console.WriteLine($"  Secret Length: {secret?.Length ?? 0}");
//        Console.WriteLine($"  Issuer: {issuer}");
//        Console.WriteLine($"  Audience: {audience}");

//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = false,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = issuer,
//            ValidAudience = audience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
//            ClockSkew = TimeSpan.Zero,
//            RequireExpirationTime = false
//        };

//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = context =>
//            {
//                Console.WriteLine($"[API GATEWAY] Authentication FAILED!");
//                Console.WriteLine($"  Exception: {context.Exception.GetType().Name}");
//                Console.WriteLine($"  Message: {context.Exception.Message}");
//                if (context.Exception.InnerException != null)
//                {
//                    Console.WriteLine($"  Inner: {context.Exception.InnerException.Message}");
//                }
//                return Task.CompletedTask;
//            },
//            OnTokenValidated = context =>
//            {
//                Console.WriteLine($"[API GATEWAY] Token VALIDATED successfully!");
//                var claims = context.Principal?.Claims;
//                if (claims != null)
//                {
//                    foreach (var claim in claims)
//                    {
//                        Console.WriteLine($"    {claim.Type}: {claim.Value}");
//                    }
//                }
//                return Task.CompletedTask;
//            },
//            OnMessageReceived = context =>
//            {
//                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

//                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
//                {
//                    var token = authHeader.Substring("Bearer ".Length).Trim();

//                    Console.WriteLine($"[API GATEWAY] Token received");
//                    Console.WriteLine($"  Length: {token.Length}");
//                    Console.WriteLine($"  Parts: {token.Split('.').Length}");
//                    Console.WriteLine($"  First 50 chars: {token.Substring(0, Math.Min(50, token.Length))}...");

//                    context.Token = token;
//                }

//                return Task.CompletedTask;
//            },
//            OnChallenge = context =>
//            {
//                Console.WriteLine($"[API GATEWAY] Challenge issued!");
//                Console.WriteLine($"  Error: {context.Error}");
//                Console.WriteLine($"  ErrorDescription: {context.ErrorDescription}");
//                return Task.CompletedTask;
//            }
//        };
//    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAngularApp");

//app.UseAuthentication();
//app.UseAuthorization();

await app.UseOcelot();

Console.WriteLine("API Gateway running on http://localhost:5000");

app.Run();