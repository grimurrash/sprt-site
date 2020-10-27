using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.XPath;

namespace NewSprt.ViewModels.FormModels
{
    public class SpecialPersonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Не указано фамилия")]
        [DisplayName("Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Не указано имя")]
        [DisplayName("Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Не указано отчество")]
        [DisplayName("Отчество")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Не указан год рождения")]
        [StringLength(4, ErrorMessage = "Год должен состоять из 4 чисел")]
        [DisplayName("Год рождения")]
        [Range(1993, 2020, ErrorMessage = "Укажите реальный год рождения")]
        public string BirthYear { get; set; }

        [Required(ErrorMessage = "Не выбран тип директивного указания")]
        [DisplayName("Тип директивного указания")]
        public int DirectiveTypeId { get; set; }

        [Required(ErrorMessage = "Не выбран военный комиссариат")]
        [DisplayName("Военный комиссариат")]
        public string MilitaryComissariatId { get; set; }

        [Required(ErrorMessage = "Не выбрано требование")]
        [DisplayName("Требование от...")]
        public int RequirementTypeId { get; set; }

        [DisplayName("Воинская часть")] public string MilitaryUnitId { get; set; }
        [DisplayName("Дата отправки")] public string SendDate { get; set; }

        [DisplayName("Примечание (идет после даты отправки)")]
        public string Notice { get; set; }
    }
}