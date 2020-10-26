using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Связь между персональщиком и призывником
    [Table("dir_find")]
    public class SpecialPersonToRecruit
    {
        [Column("id_05")] public int SpecialPersonId { get; set; }
        [Column("id")] public int RecruitId { get; set; }
        
        [ForeignKey(nameof(SpecialPersonId))] public SpecialPerson Person { get; set; }
        [ForeignKey(nameof(RecruitId))] public Recruit Recruit { get; set; }
    }
}