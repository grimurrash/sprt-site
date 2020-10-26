using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace NewSprt.Data.Zarnica.Models
{
    //Персональщики
    [Table("gsp05_d")]
    public class SpecialPerson
    {
        [Column("id")] public int Id { get; set; }
        [Column("r8012_g5")] public string MilitaryComissariatCode { get; set; }
        [Column("p005_g5")] public string LastName { get; set; }
        [Column("p006_g5")] public string FirstName { get; set; }
        [Column("p007_g5")] public string Patronymic { get; set; }
        [Column("k101_g5")] public int BirthYear { get; set; }
        [Column("k001_g5")] public DateTime? BirthDate { get; set; }
        [Column("prim_g5")] public string Notice { get; set; }
        
        [Column("data_g5")] public DateTime UpdateDate { get; set; }
        [Column("username")] public string UpdateUser { get; set; }

        [ForeignKey(nameof(MilitaryComissariatCode))]
        public MilitaryComissariat MilitaryComissariat { get; set; }

        public List<SpecialPersonToRecruit> SpecialPersonToRecruits { get; }
        public List<SpecialPersonToRequirement> SpecialPersonToRequirements { get; }

        public SpecialPerson()
        {
            SpecialPersonToRecruits = new List<SpecialPersonToRecruit>();
            SpecialPersonToRequirements = new List<SpecialPersonToRequirement>();
        }

        [NotMapped]
        public Requirement Requirement
        {
            get
            {
                if (SpecialPersonToRequirements.Count < 1) return null;

                var personalRequirement = SpecialPersonToRequirements.OrderBy(m => m.RequirementId).FirstOrDefault(m =>
                    m.Requirement.DirectiveTypeId == DirectiveType.PersonalPerson);

                if (personalRequirement == null) return SpecialPersonToRequirements.FirstOrDefault().Requirement;

                return personalRequirement.Requirement;
            }
        }

        [NotMapped] public string FullName => $"{LastName} {FirstName} {Patronymic}";

        [NotMapped]
        public string SendDateString
        {
            get
            {
                var notice = Notice.Replace("Отправка", "").Replace("отправка", "").Trim(',').Trim();
                if (string.IsNullOrEmpty(notice)) return "-";
                return notice;
            }
        }
        
        [NotMapped]
        public DateTime? SendDate {
            get
            {
                var notice = Notice.Replace("Отправка", "").Replace("отправка", "").Trim(',').Trim();
                var noticeArray = notice.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                DateTime sendDate;
                switch (noticeArray.Length)
                {
                    case 0:
                        break;
                    case 1:
                        if (DateTime.TryParse(notice, out sendDate))
                        {
                            return sendDate;
                        }
                        break;
                    default:
                        if (DateTime.TryParse(noticeArray[0].Trim(',').Trim('.').Trim(' '), out sendDate))
                        {
                            return sendDate;
                        }
                        break;
                }

                return null;
            }
        }
        
        
        [NotMapped]
        public string MilitaryUnitInfo
        {
            get
            {
                if (Requirement == null) return "-";

                return Requirement.MilitaryUnit.ToString();
            }
        }

        [NotMapped]
        public string RequirementInfo
        {
            get
            {
                if (Requirement == null) return "-";

                return Requirement.ToString();
            }
        }

        [NotMapped] public bool IsMark { get; set; }
    }
}