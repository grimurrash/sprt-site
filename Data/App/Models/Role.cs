using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Discription { get; set; }
    }
}