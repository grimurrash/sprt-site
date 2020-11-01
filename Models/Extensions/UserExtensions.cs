using System;
using System.Linq;
using System.Security.Claims;

namespace NewSprt.Models.Extensions
{
    public static class UserExtensions
    {
        public static string GetToken(this ClaimsPrincipal user)
        {
            return !user.HasClaim(c =>
                string.Equals(c.Type, "Token", StringComparison.CurrentCultureIgnoreCase)) ? 
                "" : 
                user.FindFirst(c => string.Equals(c.Type, "Token", StringComparison.CurrentCultureIgnoreCase)).Value;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return !user.HasClaim(c =>
                string.Equals(c.Type, "FullName", StringComparison.CurrentCultureIgnoreCase)) ? 
                "Неизвестный" : 
                user.FindFirst(c => string.Equals(c.Type, "FullName", StringComparison.CurrentCultureIgnoreCase)).Value;
        }

        public static bool IsPermission(this ClaimsPrincipal user, string permission)
        {
            if (!user.HasClaim(c =>
                string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)))
                return false;
            var permissions = user
                    .FindFirst(c => string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)).Value
                    .Split(",")
                    .ToList();
            if (permissions.Count <= 0) return false;
            if (permissions.Count(m => string.Equals(m, "Admin", StringComparison.CurrentCultureIgnoreCase)) > 0)
                return true;
            return permissions.Count(m => string.Equals(m, permission, StringComparison.CurrentCultureIgnoreCase)) > 0;
        }
        
        public static bool IsPermission(this ClaimsPrincipal user, params string[] requirementPermissions)
        {
            if (!user.HasClaim(c =>
                string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)))
                return false;
            var permissions = user
                .FindFirst(c => string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)).Value
                .Split(",")
                .ToList();
            if (permissions.Count <= 0) return false;
            if (permissions.Count(m => string.Equals(m, "Admin", StringComparison.CurrentCultureIgnoreCase)) > 0)
                return true;
            return permissions.Count(m => requirementPermissions.Select(r => r.ToLower()).Contains(m.ToLower())) > 0;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            if (!user.HasClaim(c =>
                string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)))
                return false;
            var permissions = user
                .FindFirst(c => string.Equals(c.Type, "Permissions", StringComparison.CurrentCultureIgnoreCase)).Value
                .Split(",")
                .ToList();
            if (permissions.Count <= 0) return false;
            return permissions.Count(m => string.Equals(m, "Admin", StringComparison.CurrentCultureIgnoreCase)) > 0;
        }
    }
}