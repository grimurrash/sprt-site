using System.ComponentModel.DataAnnotations;

namespace NewSprt.ViewModels.FormModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан Логин")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Не указан Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}