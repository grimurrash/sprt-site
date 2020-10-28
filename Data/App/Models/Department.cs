using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.App.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int HeadUserId { get; set; }
        [ForeignKey(nameof(HeadUserId))]
        public User HeadUser { get; set; } 
    }
}