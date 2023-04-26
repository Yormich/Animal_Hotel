﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Animal_Hotel.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, ConfigurationManager config)
        {
            services.AddControllersWithViews();

            //Authentication and authorization
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidAudience = config["Jwt:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                        ValidateIssuer = true,
                        ValidIssuer = config["Jwt:Issuer"],
                    };
                });


            services.AddAuthorization();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
            });
            services.AddHttpContextAccessor();

            services.AddMemoryCache();

            services.AddSingleton<IIFileProvider, BaseFileProvider>();

            services.AddScoped<ClaimHelper>();

            //Database services
            services.AddDbContext<AnimalHotelDbContext>();
            services.AddSingleton<IDbConnectionProvider, MsSqlConnectionProvider>();
            services.AddScoped<IUserLoginInfoService, UserLoginInfoService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IUserRegisterService, UserRegisterService>();
            services.AddScoped<IRoomService, RoomService>();
        }
    }
}
