using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Наименование директивных указаний
    [Table("dir_type")]
    public class DirectivesType
    {
        [Key] [Column("id")] public int Id { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("viewname")] public string ViewName { get; set; }
    }
}