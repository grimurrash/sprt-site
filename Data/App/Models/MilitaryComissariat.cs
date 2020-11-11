using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    public class MilitaryComissariat
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string InnerCode { get; set; }
        public string ShortName { get; set; }
    }
}