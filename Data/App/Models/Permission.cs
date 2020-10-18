using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NewSprt.Data.App.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Discription { get; set; }
        public List<UserPermission> UserPermissions { get; set; }
        public Permission()
        {
            UserPermissions = new List<UserPermission>();
        }
    }
}