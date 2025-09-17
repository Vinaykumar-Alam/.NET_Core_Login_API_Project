using Flutter_Backed.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Flutter_Backed.utils
{
    /// <summary>
    /// Middleware that ensures sliding expiration of refresh tokens.
    /// - Checks if user is authenticated.
    /// - If their refresh token is close to expiry, extend its lifetime.
    /// </summary>
    public class TokenMiddleware
    {
        private readonly RequestDelegate _requestDelegate; // Delegate to call the next middleware in pipeline
        private readonly IServiceProvider _serviceProvider; // Used to create scoped services (like DbContext)
        private readonly IConfiguration _configuration;     // To access configuration values (e.g., JWT settings)

        public TokenMiddleware(RequestDelegate requestDelegate, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _configuration = configuration;
            _requestDelegate = requestDelegate;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // If user is not authenticated, skip this middleware and continue
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                await _requestDelegate(httpContext);
                return;
            }

            // Create a scope for dependency injection (so we can use DbContext safely per request)
            using (var scope = _serviceProvider.CreateScope())
            {
                // Extract userId from JWT claims
                var UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Resolve SQLDBContext from the scoped services
                var dbContext = scope.ServiceProvider.GetService<SQLDBContext>();

                // Fetch refresh token details for this user from DB (synchronously blocking here - can be improved!)
                var userRefreshTokenDetails = dbContext.RefreshToken_Tbl
                    .Where(w => w.UserId == Guid.Parse(UserId))
                    .FirstOrDefaultAsync()
                    .Result;

                // Sliding expiration logic
                // If refresh token expires within the next 5 minutes, extend it
                if (userRefreshTokenDetails.ExpiresOn <= DateTime.Now.AddMinutes(5))
                {
                    // Extend expiry by another 5 minutes
                    userRefreshTokenDetails.ExpiresOn = DateTime.Now.AddMinutes(5);

                    // Update "last refreshed" timestamp
                    userRefreshTokenDetails.CreatedOn = DateTime.Now;
                }

                // Save updated token info back to DB
                dbContext.RefreshToken_Tbl.Update(userRefreshTokenDetails);
                await dbContext.SaveChangesAsync();
            }

            // Call the next middleware (or controller if this is last in pipeline)
            await _requestDelegate(httpContext);
        }
    }
}
