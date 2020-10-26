using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Информация о командах
    [Table("gsp07_n")]
    public class Team
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("p049_g7")] public string TeamNumber { get; set; }
        [Column("p302_g7")] public string DistrictTeamNumber { get; set; }
        [Column("p105_time")] public DateTime? SendDate { get; set; }
        [Column("p113_g7")] public string City { get; set; }
        [Column("r6205_g7")] public string Gsh { get; set; }
        [Column("r7012_g7")] public string ArmyTypeId { get; set; }
        [Column("p107_g7")] public string MilitaryUnitCode { get; set; }
        [Column("r9004_g7")] public string MilitaryDistrictCode { get; set; }
        [Column("psh_code")] public string PatronageRelations { get; set; }
        [Column("p102_g7")] public int Amount { get; set; }
        [Column("prim_g7")] public string Notice { get; set; }
        [Column("r4054_g7")] public string TrainingId { get; set; }
        [Column("r1016_g7")] public string ProfessionId { get; set; }
        [Column("r7147_g7")] public string CategoryId { get; set; }
        [Column("p4040_g7")] public int? CategoryNum { get; set; }
        [Column("r4076_g7")] public string FormId { get; set; }
        
        [ForeignKey(nameof(MilitaryUnitCode))] public MilitaryUnit MilitaryUnit { get; set; }
        [ForeignKey(nameof(ArmyTypeId))] public ArmyType ArmyType { get; set; }
        //[ForeigKey(nameof(Id))] невозможно указать явно
        public TeamCount TeamCount { get; set; }
        
        [ForeignKey(nameof(MilitaryDistrictCode))]
        public MilitaryDistrict MilitaryDistrict { get; set; }

        public List<Recruit> Recruits { get; set; }

        public Team()
        {
            Recruits = new List<Recruit>();
        }
    }
}