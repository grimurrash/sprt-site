using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.ViewModels.FormModels
{
    public class MedicalTestNumberEditViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [DisplayName("№ теста на COVID-19")]
        public string TestNum { get; set; }
        
        [Required]
        public DateTime TestDate { get; set; }
    }
}