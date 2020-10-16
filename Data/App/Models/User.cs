using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Fio { get; set; }
        public int DepartmentId { get; set; }
    }
}