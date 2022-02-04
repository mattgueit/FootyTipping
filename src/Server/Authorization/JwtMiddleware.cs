using FootyTipping.Server.Services;

namespace FootyTipping.Server.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IJwtUtilities jwtUtilities)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtilities.ValidateToken(token);
            if (userId != null)
            {
                // Attach user to context on successful jwt validation
                context.Items["User"] = userService.GetById(userId.Value);
            }

            await _next(context);
        }
    }
}
