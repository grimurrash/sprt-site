using System.Collections.Generic;
using NewSprt.Data.Zarnica.Models;

namespace NewSprt.ViewModels.SpecialGuidance
{
    public class TeamWithSpecialPerson
    {
        public Team MainTeam { get; set; }
        public List<ChildrenTeam> ChildrenTeams { get; set; }
        
        public int AllCount { get; set; }
        public int PersonsCount { get; set; }
        public int PatronageRecruitsCount { get; set; }
        public int RemainCount { get; set; }
        
        public TeamWithSpecialPerson()
        {
            ChildrenTeams = new List<ChildrenTeam>();
            AllCount = 0;
        }
    }
}