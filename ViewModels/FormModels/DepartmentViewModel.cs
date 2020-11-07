using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.ViewModels.FormModels
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }
        [DisplayName("Аббревиатура")]
        [Required(ErrorMessage = "Не указано аббревиатура")]
        public string ShortName { get; set; }
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Не указано наименование отделения")]
        public string Name { get; set; }
        [DisplayName("Начальник")]
        [Required(ErrorMessage = "Не выбран начальник отделения")]
        public int? HeadUserId { get; set; }
    }
}