using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.Zarnica;

namespace NewSprt.Controllers
{
    public class MilitaryUnitController : Controller
    {
        private ZarnicaDbContext _zarnicaDb;

        public MilitaryUnitController(ZarnicaDbContext zarnicaDb)
        {
            _zarnicaDb = zarnicaDb;
        }

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
            var sendDates = teamsSendDate.Select(m => new
            {
                Value = m.ToShortDateString(),
                Text = m.ToShortDateString()
            }).ToList();
            return new JsonResult(new {isSucceeded = true, result = sendDates});
        }
    }
}