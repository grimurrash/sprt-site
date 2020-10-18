using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Информация о призывнике (паспорт, военник, банковские номера, ПЭК)
    [Table("vbilet")]
    public class AdditionalData
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("p012")] public string BirthPlace { get; set; }
        [Column("ulica")] public string Street { get; set; }
        [Column("p070")] public string House { get; set; }
        [Column("p080")] public string Building { get; set; }
        [Column("p081")] public string Apartment { get; set; }
        [Column("p008")] public string PassportSeries { get; set; }
        [Column("p099")] public string PassportNumber { get; set; }
        [Column("p022")] public string PassportIssueOrganization { get; set; }
        [Column("p010")] public DateTime PassportIssueDate { get; set; }
        [Column("vagon")] public string Note { get; set; }
        [Column("num_spr")] public string TestNum { get; set; }
        [Column("date_spr")] public DateTime? TestDate { get; set; }
    }
}