using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Server.API.Data;
using Server.API.Models;
using Server.API.Hubs;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Server.API.Middleware;
using Server.API.Services;
using Microsoft.AspNetCore.Authorization;
using Server.API.Repositories;
using Server.API.Repository;
using Server.API.Repository.Interfaces;
using Server.API.Services.Interfaces;
using Server.API.Repositories.Interfaces;
using Server.API.Games;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // tilføjer muligheden for at autentificere med JWT -->
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    // <--
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Add SignalR
builder.Services.AddSignalR();

//logging
builder.Host.UseSerilog((ctx, lc) =>
{
    // Aktiver kun Serilog i bestemte miljøer
    if (ctx.HostingEnvironment.IsDevelopment() || ctx.HostingEnvironment.IsProduction())
    {
        lc.ReadFrom.Configuration(ctx.Configuration);
        lc.WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30);
        lc.WriteTo.MSSqlServer(
            connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
            sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents", AutoCreateSqlTable = true }
        );
        lc.WriteTo.Seq("http://localhost:5341"); // Erstat med den faktiske adresse til din Seq server
        lc.Enrich.WithMachineName();
        lc.Enrich.WithThreadId();
    }
});

addJWTAuthentication(builder);

ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
// Tilføjer muligheden for at autentificere med JWT -->
app.UseAuthentication();
// <--

app.UseAuthorization();

ConfigureMiddleware(app);

//app.MapHub<HangmanHub>("/HangmanGame");
//app.MapHub<ChatAppHub>("/ChatAppHub");

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapHub<LobbyHub>(builder.Configuration["ConnectionSettings:LobbyEndpoint"]);
    _ = endpoints.MapHub<FriendsHub>(builder.Configuration["ConnectionSettings:FriendsEndpoint"]);
    _ = endpoints.MapHub<HangmanHub>(builder.Configuration["ConnectionSettings:HangmanEndpoint"]);
    _ = endpoints.MapHub<ChatAppHub>(builder.Configuration["ConnectionSettings:ChatAppEndpoint"]);
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var env = services.GetRequiredService<IWebHostEnvironment>();
    
    if (!env.IsEnvironment("Testing"))
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.DeleteOldLogEntriesAsync();
    }
}


app.Run();


void addJWTAuthentication(WebApplicationBuilder builder)
{
    // Tilføjer konfiguration
    var azureAdConfig = builder.Configuration.GetSection("AzureAd");

    // Sætter miljøvariabler baseret på konfigurationen
    Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", azureAdConfig["ClientId"]);
    Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", azureAdConfig["ClientSecret"]);
    Environment.SetEnvironmentVariable("AZURE_TENANT_ID", azureAdConfig["TenantId"]);

    var keyVaultUrl = builder.Configuration["AzureKeyVault:Endpoint"];

    var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

    var jwtKeySecret = secretClient.GetSecret("JwtKey").Value.Value;
    var jwtIssuerSecret = secretClient.GetSecret("JwtIssuer").Value.Value;
    var jwtAudienceSecret = secretClient.GetSecret("JwtAudience").Value.Value;

    builder.Configuration["Jwt:Key"] = jwtKeySecret;
    builder.Configuration["Jwt:Issuer"] = jwtIssuerSecret;
    builder.Configuration["Jwt:Audience"] = jwtAudienceSecret;

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                options.DefaultChallengeScheme =
                    options.DefaultForbidScheme =
                        options.DefaultScheme =
                            options.DefaultSignInScheme =
                                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine("Authentication failed: " + context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Token validated");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole("User") // Angiv de roller, som standard autentificerede brugere skal have
            .Build();

        options.AddPolicy("Guest+", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("Guest") || context.User.IsInRole("User")));

        options.AddPolicy("User+", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("User")));
    });
}

void ConfigureMiddleware(WebApplication app)
{
    app.Use(async (context, next) =>
    {
        using (var scope = app.Services.CreateScope())
        {
            var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
            var middleware = new TokenRefreshMiddleware(next, jwtTokenService);
            await middleware.InvokeAsync(context);
        }
    });
    app.UseMiddleware<ErrorLoggingMiddleware>();
}

void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<ITokenRepository, TokenRepository>();
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<ITimeService, TimeService>();
    services.AddSingleton<IIdGenerator, RandomGenerator>();
    services.AddScoped<IFriendsRepository, FriendsRepository>();
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IGameRepository, GameRepository>();
    services.AddSingleton<ILobbyManager, LobbyManager>();
    services.AddScoped<IRandomPicker, RandomGenerator>();
}


public partial class Program
{
}