using NewSprt.Data.Zarnica.Models;

namespace NewSprt.ViewModels.SpecialGuidance
{
    /// <summary>
    /// Класс для вывода информации об шефских связях
    /// </summary>
    public class PatronageTask
    {
        public MilitaryComissariat MilitaryComissariat { get; set; }
        public int Count { get; set; }
    }
}