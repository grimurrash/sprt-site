using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NewSprt.Data.Zarnica.Models
{
    [Table("gsp01_ur")]
    public class Recruit
    {
        [Column("id")] public int Id { get; set; }
        [Column("pnom")] public string Code { get; set; }
        [Column("p005")] public string FirstName { get; set; }
        [Column("p006")] public string LastName { get; set; }
        [Column("p007")] public string Patronymic { get; set; }
        [Column("k001")] public DateTime BirthDate { get; set; }
        [Column("r8012")] public string MilitaryComissariatId { get; set; }
        [Column("p097")] public string MobileCode { get; set; }
        [Column("p098")] public string MobileNumber { get; set; }
        [Column("id_07")] public int? TeamId { get; set; }
        [Column("p049_g2")] public string TeamNumber { get; set; }
        [Column("p113_g2")] public string DestinationStation { get; set; }
        [Column("p105_g2")] public DateTime? SendDate { get; set; }
        [Column("id")] public int AdditionalDataId { get; set; }
        [Column("r6101")] public string SettlementCode { get; set; }

        [ForeignKey("SettlementCode")] public Settlement Settlement { get; set; }
        [ForeignKey("AdditionalDataId")] public AdditionalData AdditionalData { get; set; }
        [ForeignKey("TeamId")] public Team Team { get; set; }
        [ForeignKey(nameof(MilitaryComissariatId))]
        public MilitaryComissariat MilitaryComissariat { get; set; }

        public List<EventControl> Events { get; set; }
        public List<SpecialPersonToRecruit> RecruitFromSpecialPersons { get; set; }

        public Recruit()
        {
            Events = new List<EventControl>();
            RecruitFromSpecialPersons = new List<SpecialPersonToRecruit>();
        }

        public EventControl LastEvent => Events.OrderBy(m => m.Id).Last();
    }
}