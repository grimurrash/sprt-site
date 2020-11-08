using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.ViewModels.FormModels
{
    public class ConscriptionPeriodViewModel
    {
        public int Id { get; set; }
        
        [DisplayName("Наименование периода")]
        [Required(ErrorMessage = "Не указан наименование периода призыва")]
        public string Name { get; set; }
        
        [DisplayName("В архиве")]
        public bool IsArchive { get; set; }
    }
}