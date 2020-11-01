using System.Collections.Generic;

namespace NewSprt.Models.Requirements
{
    public static class AppPermissionPolicy
    {
        private static readonly List<string> PermissionPolicyName = new List<string>
        {
            "VVK", "Dactyloscopy", "PersonalGuidance", "Secretary"
        };

        public static IEnumerable<string> PolicyNameList()
        {
            return PermissionPolicyName;
        }
    }
}