﻿using System;
using System.Collections.Generic;
using System.Linq;
using NewSprt.Data.Zarnica.Models;

namespace NewSprt.ViewModels.SpecialGuidance
{
    public class ChildrenTeam
    {
        public string Title { get; set; }
        
        public List<SpecialPerson> Persons { get; set; }
        public List<PatronageTask> PatronageTasks { get; set; }
        
        public int AllCount { get; set; }
        public int PersonsCount { get; set; }
        public int PatronageTasksCount { get; set; }
        public int RemainCount { get; set; }

        public string GetPatronageTasksText()
        {
            if (PatronageTasks.Count == 0) return "";
            return string.Join(", ", PatronageTasks.Select(m => $"{m.MilitaryComissariat.ShortName} - {m.Count} чел."));
        }

        public string GetPatrinageCount()
        {
            return PatronageTasksCount +
                   (PatronageTasks.Sum(m => m.Count) == PatronageTasksCount ? "" : $" ({PatronageTasks.Sum(m => m.Count)})");
        }

        public ChildrenTeam()
        {
            Persons = new List<SpecialPerson>();
            PatronageTasks = new List<PatronageTask>();
        }
    }
}