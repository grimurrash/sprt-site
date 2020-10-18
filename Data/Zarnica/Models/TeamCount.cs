using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Кол-во старших команд в команде
    [Table("gsp07_kol")]
    public class TeamCount
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("data")] public DateTime Date { get; set; }
        [Column("sendall")] public int AllCount { get; set; }
        [Column("ofic")] public int ОfficersCount { get; set; }
        [Column("predstav")] public int PepresentativeId { get; set; }
        [Column("serg")] public int SergeantsCount { get; set; }
        [Column("prap")] public int PraporshikCount { get; set; }

        [ForeignKey(nameof(Id))] public Team Team { get; set; }
    }
}