using System.Security.Claims;

namespace TaskManager.Services
{
    public class UserService : IUserService
    {
        private HttpContext httpContext;
        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext!;
        }
        public string GetUserId()
        {
            if (httpContext.User is not null && httpContext.User.Identity is not null &&
                httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims
                    .Where(x => x.Type.Equals(ClaimTypes.NameIdentifier))
                    .FirstOrDefault();

                return idClaim is not null ? idClaim.Value : "";
            }
            else
            {
                throw new Exception("The user is not authenticated");
            }
        }
    }
}
