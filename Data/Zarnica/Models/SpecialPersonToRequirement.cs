using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Связь между Персональщиком и директивным указанием
    [Table("dir_zap")]
    public class SpecialPersonToRequirement
    {
        [Column("id_05")] public int PersonId { get; set; }
        [Column("id")] public int RequirementId { get; set; }
        
        [ForeignKey(nameof(PersonId))] public SpecialPerson Person { get; set; }
        [ForeignKey(nameof(RequirementId))] public Requirement Requirement { get; set; }
    }
}