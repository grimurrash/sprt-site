using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Требование от...
    [Table("nach")]
    public class RequirementType
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("psevd")] public string Name { get; set; }
        
        [NotMapped] public const int VkrtRequirement = 2;
        [NotMapped] public const int TcpRequirement = 60;
    }
}