using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    public class MilitaryComissariat
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string InnerCode { get; set; }
        public string ShortName { get; set; }
        
        public List<Recruit> Recruits { get; set; }

        public MilitaryComissariat()
        {
            Recruits = new List<Recruit>();
        }
        
        public string GetDocumentName()
        {
            var text = Name;
            text = text.Replace("Республика", "Респ.");
            text = text.Replace("Республики", "Респ.");
            text = text.Replace("область", "обл.");
            text = text.Replace("Область", "обл.");
            text = text.Replace("район", "р-н");
            text = text.Replace("Район", "р-н");
            text = text.Replace("город", "г.");
            text = text.Replace("Город", "г.");
            text = text.Replace("поселок", "пос.");
            text = text.Replace("Поселок", "пос.");
            text = text.Replace("улица", "ул.");
            text = text.Replace("Улица", "ул.");
            text = text.Replace("Дом", "д.");
            text = text.Replace("корпус", "корп.");
            text = text.Replace("деревня", "д.");
            text = text.Replace("Деревня", "д.");
            text = text.Replace("село", "с.");
            text = text.Replace("Село", "с.");
            text = text.Replace("г.а", "г.");
            text = text.Replace("проспект", "просп.");
            text = text.Replace("Проспект", "просп.");
            text = text.Replace("ВК ", "Военный комиссариат (") + ")";
            return text;
        }
    }
}