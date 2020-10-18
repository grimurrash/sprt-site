using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Населенный пункт
    [Table("r6101")]
    public class Settlement
    {
        [Key] [Column("p00")] public string Code { get; set; }
        [Column("p01")] public string Name { get; set; }
    }
}