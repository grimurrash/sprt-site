using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.App.Models
{
    public class ConscriptionPeriod
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public bool IsArchive { get; set; }
        
        [NotMapped] 
        public int RecruitsCount { get; set; }
    }
}