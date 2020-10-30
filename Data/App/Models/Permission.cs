using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    /// <summary>
    /// Таблица прав доступа
    /// </summary>
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