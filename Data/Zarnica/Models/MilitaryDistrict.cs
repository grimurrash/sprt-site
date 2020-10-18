using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Наименование оперативно-стратегических, оперативных и оперативно-тактических объединений. Военные округи и флоты.
    [Table("r9004")]
    public class MilitaryDistrict
    {
        [Key] [Column("p00")] public string Id { get; set; }
        [Column("p01")] public string Name { get; set; }
        [Column("p02")] public string ShortName { get; set; }
    }
}