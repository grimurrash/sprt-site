using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.Zarnica.Models
{
    //Наименование директивных указаний
    [Table("dir_type")]
    public class DirectiveType
    {
        [Column("id")] public int Id { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("viewname")] public string ViewName { get; set; }

        [NotMapped] public const int PersonalPerson = 12;
        [NotMapped] public const int FamilyPerson = 13;
        [NotMapped] public const int SpecialistPerson = 10;
        [NotMapped] public const int SelectedToTheMilitaryUnitPerson = 5;
    }
}