using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.App.Models
{
    /// <summary>
    /// Таблица для связи пользователя и прав доступа
    /// </summary>
    public class UserPermission
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("PermissionId")]

        public Permission Permission { get; set; }
    }
}