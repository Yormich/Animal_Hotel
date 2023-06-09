﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.ResponseCompression;
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

            //TODO: uncomment code when app is ready to be developed
            /*services.AddResponseCompression((options) =>
            {
                options.EnableForHttps = true;

              //options.Providers.Add(new GzipCompressionProvider(new GzipCompressionProviderOptions()));
            });*/

            services.AddSingleton<IIFileProvider, BaseFileProvider>();

            services.AddScoped<ClaimHelper>();

            //Database services
            services.AddDbContext<AnimalHotelDbContext>();
            
            services.AddSingleton<IDbConnectionProvider, MsSqlConnectionProvider>();
            services.AddSingleton<IImagePathProvider, ImagePathProvider>();

            //for correct injection in room service
            services.AddTransient<IEnclosureService, EnclosureService>();

            services.AddScoped<IUserLoginInfoService, UserService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IUserTypeService, UserTypeService>();
            services.AddScoped<IReviewService, ReviewService>();

            services.AddTransient<IRequestService, RequestService>();
            services.AddTransient<IAnimalService, AnimalService>();
            services.AddTransient<IContractService, ContractService>();
            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IEnclosureService, EnclosureService>();
            services.AddTransient<IBookingService, BookingService>();
        }
    }
}
