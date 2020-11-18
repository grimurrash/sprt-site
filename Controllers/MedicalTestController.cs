using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.App;
using NewSprt.Data.App.Models;
using NewSprt.Data.Zarnica;
using NewSprt.Models.Extensions;
using NewSprt.Models.Helper.Documents;
using NewSprt.Models.Managers;
using NewSprt.ViewModels;

namespace NewSprt.Controllers
{
    [Authorize(Policy = Permission.Vvk)]
    public class MedicalTestController : Controller
    {
        private readonly AppDbContext _appDb;
        private readonly ZarnicaDbContext _zarnicaDb;
        private readonly RecruitManager _recruitManager;

        public MedicalTestController(AppDbContext appDb, ZarnicaDbContext zarnicaDb)
        {
            _appDb = appDb;
            _zarnicaDb = zarnicaDb;
            _recruitManager = new RecruitManager(appDb, zarnicaDb);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MilitaryComissariats = await _appDb.MilitaryComissariats.AsNoTracking().ToListAsync();
            var conscriptionPeriodId =
                (await _appDb.ConscriptionPeriods.AsNoTracking().FirstOrDefaultAsync(c => !c.IsArchive)).Id;
            ViewBag.ConscriptionPeriodId = conscriptionPeriodId;
            ViewData["recruitsCount"] = await _appDb.Recruits.AsNoTracking().CountAsync(m =>
                m.ConscriptionPeriodId == conscriptionPeriodId);
            await _recruitManager.SynchronizationOfDatabases();
            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public async Task<IActionResult> IndexGrid(
            string militaryComissariatId = "",
            int conscriptionPeriodId = 0,
            int page = 1,
            int rows = 20,
            bool isNotNumber = false,
            string search = "")
        {
            ViewBag.Pagination = new Pagination(rows, page);
            var query = _appDb.Recruits.Include(m => m.MilitaryComissariat).AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                query = query.Where(m => m.MilitaryComissariatCode == militaryComissariatId);
            }

            if (conscriptionPeriodId != 0)
            {
                query = query.Where(m => m.ConscriptionPeriodId == conscriptionPeriodId);
            }

            var appRecruits = await query.AsNoTracking()
                .OrderByDescending(m => m.DeliveryDate)
                .ThenBy(m => m.MilitaryComissariatCode).ThenBy(m => m.LastName).ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                var searchArr = search.Split(" ");
                switch (searchArr.Length)
                {
                    case 1:
                        appRecruits = appRecruits.Where(m => m.LastName.ToLower().StartsWith(search.ToLower()))
                            .ToList();
                        break;
                    case 2:
                        appRecruits = appRecruits.Where(m =>
                            m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.FirstName.ToLower().StartsWith(searchArr[1].ToLower())).ToList();
                        break;
                    case 3:
                        appRecruits = appRecruits.Where(m =>
                            m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.FirstName.ToLower().StartsWith(searchArr[1].ToLower()) &&
                            m.Patronymic.ToLower().StartsWith(searchArr[2].ToLower())).ToList();
                        break;
                }
            }

            var recruitsIds = appRecruits.Select(r => r.RecruitId);
            var zRecruits = await _zarnicaDb.Recruits
                .Include(m => m.AdditionalData)
                .Include(m => m.MilitaryComissariat)
                .Where(m => recruitsIds.Contains(m.Id))
                .AsNoTracking().ToListAsync();
            foreach (var recruit in zRecruits)
            {
                appRecruits.First(m => m.RecruitId == recruit.Id).ZRecruit = recruit;
                if (string.IsNullOrEmpty(recruit.AdditionalData.TestNum) ||
                    recruit.AdditionalData.TestDate == null ||
                    !Regex.IsMatch(recruit.AdditionalData.TestNum, @"^\d{1,2}/\d{1,4}",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    recruit.AdditionalData.IsNotTest = true;
                }
            }

            if (isNotNumber) appRecruits = appRecruits.Where(m => m.ZRecruit.AdditionalData.IsNotTest).ToList();

            return PartialView("_IndexGrid", appRecruits);
        }

        public async Task<IActionResult> MedicalTestProtocolModal()
        {
            var teams = await _zarnicaDb.Teams.Select(m => m.TeamNumber).Where(m => m != null).AsNoTracking()
                .ToListAsync();
            var teamNumberList = teams.Select(m => m.TrimEnd('а', 'б', 'в', 'г', 'д', 'е', 'ж')).Distinct().ToList();
            return PartialView("_MedicalTestProtocolModal", teamNumberList);
        }

        public async Task<IActionResult> PrintMedicalTestProtocol(string teamNumber)
        {
            teamNumber = teamNumber.TrimEnd('а', 'б', 'в', 'г', 'д', 'е', 'ж');
            var numbers = new List<string>
            {
                teamNumber,
                teamNumber + "а",
                teamNumber + "б",
                teamNumber + "в",
                teamNumber + "г",
                teamNumber + "д",
                teamNumber + "е",
                teamNumber + "ж"
            };
            var teams = await _zarnicaDb.Teams.Where(m => numbers.Contains(m.TeamNumber)).ToListAsync();

            var dateTime = teams[0].SendDate;
            var sendDate = dateTime ?? DateTime.Today;

            var recruits = await _zarnicaDb.Recruits.Where(m =>
                    m.TeamId != null && teams.Select(t => t.Id).Contains(m.TeamId.Value))
                .Include(m => m.AdditionalData)
                .Include(m => m.MilitaryComissariat)
                .OrderBy(m => m.LastName)
                .AsNoTracking().ToListAsync();

            return File(WordDocumentHelper.GenerateMedicalTestProtocol(recruits, teamNumber, sendDate),
                WordDocumentHelper.OutputFormatType,
                $"Список призывников получивших результаты теста ВКО {teamNumber}.docx");
        }
    }
}