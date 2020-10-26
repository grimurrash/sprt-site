using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //События призывников
    [Table("gsp18_ev")]
    public class EventControl
    {
        [Column("id")] public int Id { get; set; }
        [Column("id_object")] public int RecruitId { get; set; }
        [Column("zapr_code")] public int EventCode { get; set; }
        [Column("sdate")] public DateTime Date { get; set; }

        [ForeignKey(nameof(RecruitId))] public Recruit Recruit { get; set; }
    }
}