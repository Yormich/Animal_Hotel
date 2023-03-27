var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();



//don't use any other exception handler until end 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


//enable secured http redirection and file extraction from wwwroot
app.UseHttpsRedirection();
app.UseStaticFiles();

//use routing middleware
app.UseRouting();

//middlewares for modules and login
app.UseAuthentication();
app.UseAuthorization();

//default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
