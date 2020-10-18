using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Родственники
    [Table("relatives")]
    public class Relative
    {
        [Key] [Column("id")] public int RecruitId { get; set; }
        [Column("r7107")] public string RelativeType { get; set; }
        [Column("fio")] public string Fio { get; set; }

        [ForeignKey(nameof(RecruitId))] public Recruit Recruit { get; set; }
    }
}