namespace Animal_Hotel
{
    public class AuthenticationRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? token = context.Session.GetString("access_token");

            if (token != null)
            {
                context.Request.Headers.Authorization = $"Bearer {token}";
            }
            
            await _next(context);
        }
    }
}
