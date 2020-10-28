using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NewSprt.Models.Requirements
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (!context.User.HasClaim(c =>
                string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)))
                return Task.CompletedTask;

            var permissions = context.User
                .FindFirst(c => string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)).Value
                .Split(",")
                .ToList();
            if (permissions.Count <= 0) return Task.CompletedTask;
            if (permissions.Count(m => string.Equals(m, "Admin", StringComparison.CurrentCultureIgnoreCase)) > 0)
                context.Succeed(requirement);

            if (permissions.Count(m =>
                string.Equals(m, requirement.Permissions, StringComparison.CurrentCultureIgnoreCase)) > 0)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}