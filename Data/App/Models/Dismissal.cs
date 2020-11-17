using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.App.Models
{
    public class Dismissal
    {
        public int Id { get; set; }
        public int RecruitId { get; set; }
        public DateTime SendDismissalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string Notice { get; set; }
        public int LastEventCode { get; set; }
        public DateTime LastEventDate { get; set; }
        
        [NotMapped] public bool IsSend { get; set; }
        [NotMapped] public bool IsReturn { get; set; }
        
        [ForeignKey(nameof(RecruitId))]
        public Recruit Recruit { get; set; }

        public string ZRecruitStatus => Recruit.ZRecruit.Status;
    }
}