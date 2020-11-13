using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using NewSprt.Models;

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

        public List<SpecialPersonToRecruit> SpecialPersonToRecruits { get; set; }
        public List<SpecialPersonToRequirement> SpecialPersonToRequirements { get; }

        public SpecialPerson()
        {
            SpecialPersonToRecruits = new List<SpecialPersonToRecruit>();
            SpecialPersonToRequirements = new List<SpecialPersonToRequirement>();
        }

        [NotMapped]
        public Recruit Recruit => SpecialPersonToRecruits.Any() ? SpecialPersonToRecruits.First().Recruit : null;

        [NotMapped]
        public Requirement Requirement
        {
            get
            {
                if (SpecialPersonToRequirements.Count < 1) return null;

                var personalRequirement = SpecialPersonToRequirements.OrderBy(m => m.RequirementId).FirstOrDefault(m =>
                    m.Requirement.DirectiveTypeId == DirectiveType.PersonalPerson);

                return personalRequirement == null
                    ? SpecialPersonToRequirements.OrderBy(m => m.RequirementId).FirstOrDefault()?.Requirement
                    : personalRequirement.Requirement;
            }
        }

        [NotMapped] public string FullName => $"{LastName} {FirstName} {Patronymic}";

        [NotMapped]
        public string SendDateString
        {
            get
            {
                var notice = Notice
                    .Replace("Отправка", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("Отправка ", "", StringComparison.OrdinalIgnoreCase)
                    .Trim(',').Trim();
                return string.IsNullOrEmpty(notice) ? "-" : notice;
            }
        }

        [NotMapped]
        public DateTime? SendDate
        {
            get
            {
                var notice = Notice
                    .Replace("Отправка", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("Отправка ", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("ДМО", "", StringComparison.OrdinalIgnoreCase)
                    .Trim(',').Trim();
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


        [NotMapped] public string MilitaryUnitInfo => Requirement == null ? "-" : Requirement.MilitaryUnit.ToString();

        [NotMapped] public string RequirementInfo => Requirement == null ? "-" : Requirement.ToString();
        
        [NotMapped] public bool IsDmo => Notice.IndexOf("ДМО", StringComparison.OrdinalIgnoreCase) > -1;
        [NotMapped] public bool IsMark { get; set; }
        [NotMapped] public bool IsDelivered { get; set; }

        public string GetRecruitStatus()
        {
            if (Recruit == null) return "Не доставлен на сборный пункт";
            if (Requirement == null) return "Ошибка при получении требования!!!";
            var lastEvent = Recruit.Events.FirstOrDefault(m => m.Date == Recruit.Events.Max(e => e.Date));
            if (lastEvent == null) return "Отсутствуют события у призывника";

            if (lastEvent.EventCode == 113 || lastEvent.EventCode == 112)
            {
                return
                    $"{EventType.GetName(lastEvent.EventCode)}: " +
                    $"{Recruit.Team.TeamNumber} (в/ч {Recruit.Team.MilitaryUnitCode} ({Recruit.Team.MilitaryUnit.Name}) " +
                    $"на {Recruit.Team.SendDate?.ToShortDateString()})";
            }
            return EventType.GetName(lastEvent.EventCode);
        }
    }
}