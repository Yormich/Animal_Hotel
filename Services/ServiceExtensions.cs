using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            services.AddScoped<IUserLoginInfoService, UserService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IUserRegisterService, UserRegisterService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IUserTypeService, UserTypeService>();
            services.AddTransient<IRequestService, RequestService>();
            services.AddTransient<IAnimalService, AnimalService>();
        }
    }
}
