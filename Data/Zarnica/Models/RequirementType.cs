using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Требование от...
    [Table("nach")]
    public class RequirementType
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("psevd")] public string FullName { get; set; }
    }
}