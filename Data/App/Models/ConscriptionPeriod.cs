using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    public class ConscriptionPeriod
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public bool IsArchive { get; set; }

        public List<Recruit> Recruits { get; set; }

        public ConscriptionPeriod()
        {
            Recruits = new List<Recruit>();
        }
    }
}