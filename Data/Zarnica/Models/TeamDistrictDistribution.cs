using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Распределение призывников (военных комиссариатов) по командам
    [Table("gsp07_sp")]
    public class TeamDistrictDistribution
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("p049_g7")] public string TeamNumber { get; set; }
        [Column("p302_g7")] public string DistrictTeamNumber { get; set; }
        [Column("p102_all")] public int? AllCount { get; set; }
        [Column("p102_g7")] public int? Count { get; set; }
        [Column("r7703_g7")] public string Education { get; set; }
        [Column("p113_g7")] public string City { get; set; }
        [Column("p264_g7")] public int? TeamId { get; set; }
        [Column("r8012_g7")] public string MilitaryComissariatId { get; set; }
        [Column("p105_time")] public DateTime SendDate { get; set; }
        [Column("p107_g7")] public string MilitaryUnitCode { get; set; }
        [Column("r4054_g7")] public string TrainingId { get; set; }
        [Column("r1016_g7")] public string ProfessionId { get; set; }
        [Column("r7147_g7")] public string CategoryId { get; set; }

        [ForeignKey("MilitaryComissariatId")] public MilitaryComissariat MilitaryComissariat { get; set; }
        [ForeignKey("MilitaryUnitCode")] public MilitaryUnit MilitaryUnit { get; set; }
    }
}