using System.Net;
using System.Text.Json;
using WebApi.Helpers;

namespace FootyTipping.Server.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                // TODO: check out Nick Chapsas video on handling this. Or is this already the new way to do it?
                switch (ex)
                {
                    case AppException e:
                        // Custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case KeyNotFoundException e:
                        // Not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // Unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new { message = ex?.Message });
                await response.WriteAsync(result);

            }
        }
    }
}
