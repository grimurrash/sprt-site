using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.Zarnica;

namespace NewSprt.Controllers
{
    public class MilitaryUnitController : Controller
    {
        /// <summary>
        /// Context для работы с базой Зарницы
        /// </summary>
        private readonly ZarnicaDbContext _zarnicaDb;

        public MilitaryUnitController(ZarnicaDbContext zarnicaDb)
        {
            _zarnicaDb = zarnicaDb;
        }
        
        /// <summary>
        /// Вывод списка дат отправок команд в воинскую часть 
        /// </summary>
        /// <param name="id">Id воинской части</param>
        /// <returns>Json список с датами отправок команд в воинскую часть</returns>
        public JsonResult SendDateJsonList(string id)
        {
            if (_zarnicaDb.MilitaryUnits.AsNoTracking().Count(m => m.Id == id) == 0)
                return new JsonResult(new {isSucceeded = false});

            var teamsSendDate = _zarnicaDb.Teams
                .Where(m => m.MilitaryUnitCode == id && m.SendDate != null)
                .AsNoTracking()
                .Select(m => m.SendDate.Value)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
            var sendDates = teamsSendDate.Select(m => m.ToShortDateString()).ToList();
            return new JsonResult(new {isSucceeded = true, result = sendDates});
        }
    }
}