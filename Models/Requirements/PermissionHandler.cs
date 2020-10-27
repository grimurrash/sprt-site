using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NewSprt.Models.Requirements
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type.ToLower() == "Permissions".ToLower()))
            {
                var permissions = context.User.FindFirst(c => c.Type.ToLower() == "Permissions".ToLower()).Value.Split(",").ToList();
                if (permissions.Count > 0)
                {
                    if (permissions.Count(m => m.ToLower() == "Admin".ToLower()) > 0) context.Succeed(requirement);
                    if (permissions.Count(m => m.ToLower() == requirement.Permissions.ToLower()) > 0) context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}