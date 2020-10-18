using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Представитель воинской части
    [Table("predstav")]
    public class Predstav
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("vchast")] public string MilitaryUnitId { get; set; }
        [Column("address")] public string Address { get; set; }
        [Column("fio")] public string FullName { get; set; }
        [Column("fio_kom")] public string MilitaryUnitСommanderFullName { get; set; }
        
        [ForeignKey(nameof(MilitaryUnitId))] public MilitaryUnit MilitaryUnit { get; set; }
    }
}