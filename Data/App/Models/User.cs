using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.App.Models
{
    /// <summary>
    /// Таблица пользователей
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string AuthorizationToken { get; set; }
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; }
        public List<UserPermission> UserPermissions { get;}
        public User()
        {
            UserPermissions = new List<UserPermission>();
        }
    }
}