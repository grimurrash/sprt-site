using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Наименования военных комиссариатов
    [Table("r8012")]
    public class MilitaryComissariat
    {
        [Column("p00")] public string Id { get; set; }
        [Column("p01")] public string Name { get; set; }
        [Column("sp")] public string IsEmpty { get; set; }
        [Column("p06")] public string ShortName { get; set; }
        
        [NotMapped] public const string CurrentRegion = "1192000000";
        [Column("p03")] public string Region { get; set; }
        
        public List<Recruit> Recruits { get; set; }

        public MilitaryComissariat()
        {
            Recruits = new List<Recruit>();
        }
    }
}