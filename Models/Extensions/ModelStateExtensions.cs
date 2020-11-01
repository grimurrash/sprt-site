using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NewSprt.Models.Extensions
{
    public static class ModelStateExtensions
    {
        public static Dictionary<string, string[]> Errors(this ModelStateDictionary modelStateDictionary)
        {
            return modelStateDictionary.Where(m => m.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        }
    }
}