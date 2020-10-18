using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Военская часть
    [Table("vchast")]
    public class MilitaryUnit
    {
        [Key] [Column("p00")] public string Id { get; set; }
        [Column("p01")] public string Name { get; set; }

        public List<Predstav> Predstavs { get; set; }

        public MilitaryUnit()
        {
            Predstavs = new List<Predstav>();
        }
    }
}