using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Военская часть
    [Table("vchast")]
    public class MilitaryUnit
    {
        [Column("p00")] public string Id { get; set; }
        [Column("p01")] public string Name { get; set; }

        public List<Predstav> Predstavs { get; set; }

        public override string ToString()
        {
            
            switch (Id)
            {
                case "По указанию":
                    return "По указанию";
                case "-":
                    return "";
                default:
                    return $"{Id} ({Name})";
            }
        }

        public MilitaryUnit()
        {
            Predstavs = new List<Predstav>();
        }
    }
}