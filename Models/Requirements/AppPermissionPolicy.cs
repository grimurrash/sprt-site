using System.Collections.Generic;
using NewSprt.Data.App.Models;

namespace NewSprt.Models.Requirements
{
    public static class AppPermissionPolicy
    {
        private static readonly List<string> PermissionPolicyName = new List<string>
        {
            Permission.Admin, Permission.Dactyloscopy, Permission.PersonalGuidance, Permission.Secretary, Permission.Dismissals, Permission.Vvk
        };

        public static IEnumerable<string> PolicyNameList()
        {
            return PermissionPolicyName;
        }
    }
}