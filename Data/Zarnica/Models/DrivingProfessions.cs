using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Водительские специальности по ДООСАФ
    [Table("r4054")]
    public class DrivingProfessions
    {
        [Column("p00")] public string Id { get; set; }
        [Column("p02")] public string Name { get; set; }
    }
}