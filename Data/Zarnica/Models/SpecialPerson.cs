using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Персональщики
    [Table("gsp05_d")]
    public class SpecialPerson
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("r8012_g5")] public string MilitaryComissariatId { get; set; }
        [Column("p005_g5")] public string Surname { get; set; }
        [Column("p006_g5")] public string Name { get; set; }
        [Column("p007_g5")] public string Patronymic { get; set; }
        [Column("k101_g5")] public int BirthYear { get; set; }
        [Column("prim_g5")] public string Notice { get; set; }

        [ForeignKey(nameof(MilitaryComissariatId))]
        public MilitaryComissariat MilitaryComissariat { get; set; }
        
        public List<SpecialPersonToRecruit> SpecialPersonToRecruits { get; }
        public List<SpecialPersonToRequirement> SpecialPersonToRequirements { get; }

        public SpecialPerson()
        {
            SpecialPersonToRecruits = new List<SpecialPersonToRecruit>();
            SpecialPersonToRequirements = new List<SpecialPersonToRequirement>();
        }
    }
}