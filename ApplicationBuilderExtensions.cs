namespace Animal_Hotel
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureMiddlewares(this IApplicationBuilder app) 
        {
            app.UseSession();

            //enable secured http redirection and file extraction from wwwroot
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            //use routing middleware
            app.UseRouting();

            //middlewares for modules and login
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
