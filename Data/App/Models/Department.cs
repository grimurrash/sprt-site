using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace NewSprt.Data.App.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int HeadUserId { get; set; }
    }
}