using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using zModels = NewSprt.Data.Zarnica.Models;

namespace NewSprt.Data.App.Models
{
    public class Recruit
    {
        [Key]
        public int Id { get; set; }
        public int ConscriptionPeriodId { get; set; }
        public int RecruitId { get; set; }
        public string UniqueRecruitNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime BirthDate { get; set; }
        public string MilitaryComissariatCode { get; set; }
        
        [DefaultValue(1)]
        public int DactyloscopyStatusId { get; set; }
        
        [ForeignKey(nameof(DactyloscopyStatusId))]
        public DactyloscopyStatus DactyloscopyStatus { get; set; }

        [ForeignKey(nameof(MilitaryComissariatCode))]
        public MilitaryComissariat MilitaryComissariat { get; set; }
        
        [ForeignKey(nameof(ConscriptionPeriodId))]
        public ConscriptionPeriod ConscriptionPeriod { get; set; }
       
        
        [NotMapped] public zModels.Recruit ZRecruit { get; set; }
        [NotMapped] public string FullName => $"{LastName} {FirstName} {Patronymic}";
    }
}