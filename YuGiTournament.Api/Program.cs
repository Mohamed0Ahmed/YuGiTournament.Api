using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using YuGiTournament.Api.Data;
using YuGiTournament.Api.Identities;
using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Repositories;
using YuGiTournament.Api.Services.Abstractions;
using YuGiTournament.Api.Services;

namespace YuGiTournament.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
            builder.Services.AddScoped<IMatchRepository, MatchRepository>();
            builder.Services.AddScoped<IMatchService, MatchService>();
            builder.Services.AddScoped<IPlayerService, PlayerService>();





            builder.Services.AddDbContext<ApplicationDbContext>((options) =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("YuGiContext"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                           .AddEntityFrameworkStores<ApplicationDbContext>()
                           .AddDefaultTokenProviders();

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            #endregion




            var app = builder.Build();




            #region Configure Middleware

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            #endregion

            app.Run();
        }
    }
}
