using System.Collections.Generic;
using NewSprt.Data.Zarnica.Models;

namespace NewSprt.ViewModels
{
    /// <summary>
    /// Класс для вывода фильтров
    /// </summary>
    public class FilterDataViewModel
    {
        public List<MilitaryComissariat> MilitaryComissariats { get; set; }
        public List<DirectiveType> DirectiveTypes { get; set; }
        public List<RequirementType> RequirementTypes { get; set; }
        public List<MilitaryUnit> MilitaryUnits { get; set; }

        public FilterDataViewModel()
        {
            MilitaryComissariats = new List<MilitaryComissariat>();
            DirectiveTypes = new List<DirectiveType>();
            RequirementTypes = new List<RequirementType>();
            MilitaryUnits = new List<MilitaryUnit>();
        }
    }
}