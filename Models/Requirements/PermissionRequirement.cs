using Microsoft.AspNetCore.Authorization;

namespace NewSprt.Models.Requirements
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        protected internal string Permissions { get; }

        public PermissionRequirement(string permissions)
        {
            Permissions = permissions;
        }
    }
}