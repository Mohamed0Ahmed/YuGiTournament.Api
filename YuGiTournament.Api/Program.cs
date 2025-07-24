using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Identities;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Repositories;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace YuGiTournament.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPointMatchService, PointMatchService>();
            builder.Services.AddScoped<IPlayerService, PlayerService>();
            builder.Services.AddScoped<ILeagueResetService, LeagueResetService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<IPlayerRankingService, PlayerRankingService>();
            builder.Services.AddScoped<IClassicMatchService, ClassicMatchService>();
            builder.Services.AddScoped<IMatchServiceSelector, MatchServiceSelector>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("YuGiContext")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                           .AddEntityFrameworkStores<ApplicationDbContext>()
                           .AddDefaultTokenProviders();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder.WithOrigins("https://mohamed0ahmed.github.io", "http://localhost:4200")
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });


            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 1;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine("Challenge: " + context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            #endregion

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    await SeedAdminUser(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding admin user: {ex.Message}");
                }
            }

            #region Configure Middleware

            app.UseCors("AllowFrontend");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request Path: {context.Request.Path}");
                Console.WriteLine($"Request Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"))}");
                await next(context);
            });

            app.UseAuthentication();
            app.UseAuthorization();

            // Add a simple endpoint for testing
            app.MapGet("/", () => new { Message = "YuGi Tournament API is running!", Status = "OK", Timestamp = DateTime.UtcNow });
            app.MapGet("/api/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });
            app.MapGet("/test", () => new { Message = "Test endpoint works!", Timestamp = DateTime.UtcNow });
            app.MapGet("/api/test", () => new { Message = "API test endpoint works!", Timestamp = DateTime.UtcNow });

            app.MapControllers();

            #endregion

            await app.RunAsync();
        }

        //***************************

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "a@y.com";
            const string adminPassword = "123456";
            const string adminRole = "Admin";
            const string playerRole = "Player";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            if (!await roleManager.RoleExistsAsync(playerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(playerRole));
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FName = "Admin",
                    LName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
                else
                {
                    Console.WriteLine("Failed to create admin user");
                }
            }
        }
    }
}