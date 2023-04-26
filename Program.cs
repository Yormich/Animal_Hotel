using Animal_Hotel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Encodings;
using System.Runtime.CompilerServices;
using System.Text;
using Animal_Hotel.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

app.ConfigureMiddlewares();

//don't use any other exception handler until end 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.Map("/", (HttpContext context) =>
{
    context.Response.Redirect("AnimalHotel/Rooms");
});

app.MapControllerRoute(
    name: "main",
    pattern: "{controller}/{action}");
app.Run();