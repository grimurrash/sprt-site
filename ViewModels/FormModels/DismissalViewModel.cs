using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.ViewModels.FormModels
{
    public class DismissalViewModel
    {
        public int Id { get; set; }
        [DisplayName("Призывник")]
        [Required(ErrorMessage = "Не выбран призывник")]
        public int RecruitId { get; set; }
        [DisplayName("Дата убытия")]
        [Required(ErrorMessage = "Не выбрана дата убытия")]
        public DateTime SendDismissalDate { get; set; }
        [DisplayName("Дата прибытия")]
        [Required(ErrorMessage = "Не выбрана дата прибытия")]
        public DateTime ReturnDate { get; set; }
        [DisplayName("Примечание")]
        [Required(ErrorMessage = "Не указано примечание")]
        public string Notice { get; set; }
    }
}