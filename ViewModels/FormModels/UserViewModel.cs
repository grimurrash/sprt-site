using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.ViewModels.FormModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        [DisplayName("Логин")]
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }
        [DisplayName("Пароль")]
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DisplayName("Ф.И.О.")]
        [Required(ErrorMessage = "Не указано ФИО")]
        public string FullName { get; set; }
        [DisplayName("Отделение")]
        [Required(ErrorMessage = "Не выбрано отделение")]
        public int? DepartmentId { get; set; }
        public int[] PermissionsIds { get; set; }
    }
}