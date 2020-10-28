using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Директивное указание
    [Table("zapiski")]
    public class Requirement
    {
        [Column("id")] public int Id { get; set; }
        [Column("dir_type")] public int DirectiveTypeId { get; set; }
        [Column("num")] public string DocumentNumber { get; set; }
        [Column("nach")] public int RequirementTypeId { get; set; }
        [Column("vchast")] public string MilitaryUnitCode { get; set; }
        [Column("r7012")] public string ArmyTypeCode { get; set; }
        [Column("prim")] public string Notice { get; set; }
        [Column("username")] public string UserName { get; set; }
        [Column("data_v")] public DateTime CreateDate { get; set; }
        [Column("data")] public DateTime? UpdateDate { get; set; }
        [Column("id_07")] public int? NotInfo { get; set; }
        [Column("p102")] public int? Amount { get; set; }

        [ForeignKey(nameof(MilitaryUnitCode))] public MilitaryUnit MilitaryUnit { get; set; }
        [ForeignKey(nameof(DirectiveTypeId))] public DirectiveType DirectiveType { get; set; }
        [ForeignKey(nameof(ArmyTypeCode))] public ArmyType ArmyType { get; set; }

        [ForeignKey(nameof(RequirementTypeId))]
        public RequirementType RequirementType { get; set; }

        public List<SpecialPersonToRequirement> SpecialPersonsInRequirement { get; }

        public Requirement()
        {
            SpecialPersonsInRequirement = new List<SpecialPersonToRequirement>();
        }

        public override string ToString() => $"{DirectiveType.ViewName} №{DocumentNumber} от {RequirementType.Name}";
    }
}