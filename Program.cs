using Animal_Hotel;
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
    context.Response.Redirect("/AnimalHotel/Index");
});

app.MapControllerRoute(
    name: "main",
    pattern: "{controller}/{action}");
app.Run();