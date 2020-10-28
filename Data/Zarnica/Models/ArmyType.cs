using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Род войск
    [Table("r7012")]
    public class ArmyType
    {
        [Column("p00")] public string Id { get; set; }
        [Column("p02")] public string Name { get; set; }
    }
}