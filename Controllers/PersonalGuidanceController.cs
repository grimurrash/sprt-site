﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NewSprt.Data.Zarnica;
using NewSprt.Data.Zarnica.Models;
using NewSprt.ViewModels;
using Npgsql;

namespace NewSprt.Controllers
{
    [Authorize(Policy = "PersonalGuidance")]
    public class PersonalGuidanceController : Controller
    {
        private ZarnicaDbContext _zarnicaDb;

        public PersonalGuidanceController(ZarnicaDbContext zarnicaDb)
        {
            _zarnicaDb = zarnicaDb;
        }

        //Таблица в/ч
        public IActionResult Index()
        {
            return Content("В разработке");
        }

        //Список персональщиков
        public async Task<IActionResult> List()
        {
            var filterData = new FilterViewModel();
            filterData.MilitaryComissariats = await _zarnicaDb.MilitaryComissariats
                .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                .OrderBy(m => m.ShortName).AsNoTracking().ToListAsync();

            var requirements = _zarnicaDb.Requirements
                .Select(m => new {m.MilitaryUnit, m.RequirementType}).AsNoTracking().ToList();

            filterData.MilitaryUnits = requirements.Select(m => m.MilitaryUnit)
                .Where(m => m.Name != null).Distinct().OrderBy(m => m.Id).ToList();
            filterData.RequirementTypes = requirements.Select(m => m.RequirementType)
                .Where(m => m.Name != null).Distinct().OrderBy(m => m.Name).ToList();

            filterData.DirectiveTypes = await _zarnicaDb.DirectivesTypes.AsNoTracking().ToListAsync();
            ViewBag.AllCount = await _zarnicaDb.SpecialPersons.AsNoTracking().CountAsync();
            ViewBag.FilterData = filterData;
            return View();
        }

        public async Task<PartialViewResult> ListGrid(
            string militaryComissariatId = "",
            int directorTypeId = 0,
            int requirementTypeId = 0,
            string militaryUnitId = "",
            string search = "",
            bool isMark = false,
            int page = 1,
            int rows = 10,
            bool exitMode = false)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            if (exitMode) return PartialView("_ListGrid", new List<SpecialPerson>());

            IQueryable<SpecialPerson> query = _zarnicaDb.SpecialPersons
                .Include(m => m.MilitaryComissariat)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.DirectiveType)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.RequirementType)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.MilitaryUnit).AsQueryable();

            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                query = query.Where(m => m.MilitaryComissariatCode == militaryComissariatId);
            }

            if (directorTypeId != 0)
            {
                query = query.Where(m =>
                    m.SpecialPersonToRequirements.Count(r => r.Requirement.DirectiveTypeId == directorTypeId) > 0);
            }

            if (requirementTypeId != 0)
            {
                query = query.Where(m =>
                    m.SpecialPersonToRequirements.Count(r => r.Requirement.RequirementTypeId == requirementTypeId) > 0);
            }

            if (!string.IsNullOrEmpty(militaryUnitId))
            {
                query = query.Where(m =>
                    m.SpecialPersonToRequirements.Count(r => r.Requirement.MilitaryUnitCode == militaryUnitId) > 0);
            }

            var persons = query.OrderBy(m => m.LastName).AsNoTracking().ToList();

            if (!string.IsNullOrEmpty(search))
            {
                var searchArr = search.Split(" ");
                switch (searchArr.Length)
                {
                    case 1:
                        persons = persons.Where(m => m.LastName.ToLower().StartsWith(search.ToLower())).ToList();
                        break;
                    case 2:
                        persons = persons.Where(m =>
                            m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.FirstName.ToLower().StartsWith(searchArr[1].ToLower())).ToList();
                        break;
                    case 3:
                        persons = persons.Where(m =>
                            m.LastName.ToLower().StartsWith(searchArr[0].ToLower()) &&
                            m.FirstName.ToLower().StartsWith(searchArr[1].ToLower()) &&
                            m.Patronymic.ToLower().StartsWith(searchArr[2].ToLower())).ToList();
                        break;
                }
            }

            var allTeams = await _zarnicaDb.Teams.AsNoTracking().ToListAsync();
            foreach (var pers in persons)
            {
                var requirementTeams = allTeams.Where(m =>
                        m.SendDate != null && m.MilitaryUnitCode == pers.Requirement.MilitaryUnitCode)
                    .OrderBy(m => m.SendDate).ToList();
                var requirementTeamsSendDate = requirementTeams.Select(m => m.SendDate).Distinct().ToList();
                if (pers.SendDate != null && !requirementTeamsSendDate.Select(m => m?.DayOfYear)
                    .Contains(pers.SendDate?.DayOfYear))
                {
                    pers.IsMark = true;
                }
            }

            if (isMark) persons = persons.Where(m => m.IsMark).ToList();

            return PartialView("_ListGrid", persons);
        }

        public PartialViewResult CreateModal()
        {
            var filterData = new FilterViewModel();
            filterData.MilitaryComissariats = _zarnicaDb.MilitaryComissariats
                .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                .OrderBy(m => m.ShortName).AsNoTracking().ToList();
            filterData.RequirementTypes = _zarnicaDb.RequirementTypes.OrderBy(m => m.Name).AsNoTracking().ToList();
            filterData.MilitaryUnits = _zarnicaDb.MilitaryUnits.OrderBy(m => m.Id).AsNoTracking().ToList();
            filterData.DirectiveTypes = _zarnicaDb.DirectivesTypes.OrderBy(m => m.Id).AsNoTracking().ToList();
            ViewBag.filterData = filterData;
            ViewBag.SendDates = new List<object>();
            var viewPerson = new SpecialPersonViewModel
            {
                DirectiveTypeId = DirectiveType.PersonalPerson,
                RequirementTypeId = RequirementType.VkrtRequirement
            };
            return PartialView("_CreateModal", viewPerson);
        }

        public IActionResult Create(SpecialPersonViewModel model)
        {
            if (model.RequirementTypeId == 0)
            {
                ModelState.AddModelError("RequirementTypeId", "Не выбрано требование");
            }

            // try
            // {
                var personCount = _zarnicaDb.SpecialPersons
                    .Count(m => m.LastName == model.LastName &&
                                m.FirstName == model.FirstName &&
                                m.Patronymic == model.Patronymic &&
                                m.BirthYear == int.Parse(model.BirthYear) &&
                                m.MilitaryComissariatCode ==
                                model.MilitaryComissariatId);
                if (personCount > 0)
                {
                    ModelState.AddModelError("Id", "Персональщик с такими ФИО, годом рождения и ВК уже существуеты");
                }

                if (!ModelState.IsValid)
                {
                    var errorList = ModelState.Where(m => m.Value.Errors.Count > 0).ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return new JsonResult(new {isSucceeded = false, errors = errorList});
                }

                var transaction = _zarnicaDb.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                var newPerson = new SpecialPerson
                {
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    Patronymic = model.Patronymic,
                    BirthYear = int.Parse(model.BirthYear),
                    MilitaryComissariatCode = model.MilitaryComissariatId,
                    Notice = $"Отправка {model.SendDate} {model.Notice}",
                    UpdateDate = DateTime.Now.Date,
                    UpdateUser = User.Identity.Name.ToLower()
                };

                var query =
                    "INSERT INTO gsp05_d (id, k101_g5, data_g5, username, p006_g5, p005_g5, r8012_g5, prim_g5, p007_g5) " +
                    $"VALUES (nextval('public.gsp05_d_id_seq'::text), @BirthYear, @UpdateDate, @UpdateUser, @FirstName, @LastName, @MilitaryComissariatCode, @Notice, @Patronymic)";

                _zarnicaDb.Database.ExecuteSqlCommand(query,
                    new NpgsqlParameter("BirthYear", newPerson.BirthYear),
                    new NpgsqlParameter("UpdateDate", newPerson.UpdateDate.Date),
                    new NpgsqlParameter("UpdateUser", newPerson.UpdateUser),
                    new NpgsqlParameter("FirstName", newPerson.FirstName),
                    new NpgsqlParameter("LastName", newPerson.LastName),
                    new NpgsqlParameter("MilitaryComissariatCode", newPerson.MilitaryComissariatCode),
                    new NpgsqlParameter("Notice", newPerson.Notice),
                    new NpgsqlParameter("Patronymic", newPerson.Patronymic));

                _zarnicaDb.SaveChanges();

                var requirement = _zarnicaDb.Requirements
                    .FirstOrDefault(m => m.DirectiveTypeId == model.DirectiveTypeId &&
                                         m.RequirementTypeId == model.RequirementTypeId &&
                                         m.MilitaryUnitCode == model.MilitaryUnitId);
                if (requirement == null)
                {
                    var nextIndex = _zarnicaDb.Requirements
                        .Where(m => m.DirectiveTypeId == model.DirectiveTypeId)
                        .AsNoTracking()
                        .Select(m => int.Parse(m.DocumentNumber)).Max() + 1;
                    var newIndex = "";
                    if (nextIndex < 10) newIndex = "00" + nextIndex;
                    else if (nextIndex < 100) newIndex = "0" + nextIndex;
                    else if (nextIndex < 100) newIndex = nextIndex.ToString();

                    var armyTypeId = _zarnicaDb.Teams.AsNoTracking()
                        .FirstOrDefault(m => m.MilitaryUnitCode == model.MilitaryUnitId)?.ArmyTypeId;
                    // var newId = _zarnicaDb.Requirements.Select(m => m.Id).Max() + 1;
                    
                    requirement = new Requirement
                    {
                        DocumentNumber = newIndex,
                        ArmyTypeCode = armyTypeId,
                        CreateDate = DateTime.Now.Date,
                        UpdateDate = DateTime.Now.Date,
                        DirectiveTypeId = model.DirectiveTypeId,
                        RequirementTypeId = model.RequirementTypeId,
                        MilitaryUnitCode = model.MilitaryUnitId,
                        Amount = 1,
                        NotInfo = 0,
                        Notice = "Отправка со СП РТ",
                        UserName = User.Identity.Name.ToLower()
                    };
                    
                    query =
                        "INSERT INTO zapiski (id, num, r7012, data_v, data, dir_type, nach, vchast, p102, prim, username) " +
                        $"VALUES (nextval('public.zapiski_id_seq'::text), @DocumentNumber, @ArmyTypeCode, @CreateDate, " +
                        $"@UpdateDate, @DirectiveTypeId, @RequirementTypeId, @MilitaryUnitCode, 1, 'Отправка со СП РТ', @UserName)";

                    _zarnicaDb.Database.ExecuteSqlCommand(query,
                        new NpgsqlParameter("DocumentNumber", requirement.DocumentNumber),
                        new NpgsqlParameter("ArmyTypeCode", requirement.ArmyTypeCode),
                        new NpgsqlParameter("CreateDate", requirement.CreateDate.Date),
                        new NpgsqlParameter("UpdateDate", requirement.UpdateDate.Value.Date),
                        new NpgsqlParameter("DirectiveTypeId", requirement.DirectiveTypeId),
                        new NpgsqlParameter("RequirementTypeId", requirement.RequirementTypeId),
                        new NpgsqlParameter("MilitaryUnitCode", requirement.MilitaryUnitCode),
                        new NpgsqlParameter("UserName", requirement.UserName));
                    
                }
                else
                {
                    requirement.Amount += 1;
                    _zarnicaDb.Requirements.Update(requirement);
                }

                newPerson = _zarnicaDb.SpecialPersons.AsNoTracking()
                    .FirstOrDefault(m => m.LastName == newPerson.LastName &&
                                         m.FirstName == newPerson.FirstName &&
                                         m.Patronymic == newPerson.Patronymic &&
                                         m.BirthYear == newPerson.BirthYear &&
                                         m.MilitaryComissariatCode ==
                                         newPerson.MilitaryComissariatCode);
                requirement = _zarnicaDb.Requirements.AsNoTracking()
                    .FirstOrDefault(m => m.DirectiveTypeId == model.DirectiveTypeId &&
                                         m.RequirementTypeId == model.RequirementTypeId &&
                                         m.MilitaryUnitCode == model.MilitaryUnitId);
                _zarnicaDb.SpecialPersonToRequirements.Add(new SpecialPersonToRequirement
                    {RequirementId = requirement.Id, SpecialPersonId = newPerson.Id});
                _zarnicaDb.SaveChanges();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            // }
            // catch (Exception e)
            // {
            //     var errorList = new Dictionary<string, string[]>
            //     {
            //         {"Id", new[] {"Критическая ошибка при изменении персональщика. Обратитесь в ВЦшнику"}}
            //     };
            //     return new JsonResult(new {isSucceeded = false, errors = errorList});
            // }
        }

        public PartialViewResult EditModal(int id)
        {
            var filterData = new FilterViewModel();
            filterData.MilitaryComissariats = _zarnicaDb.MilitaryComissariats
                .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                .OrderBy(m => m.ShortName).AsNoTracking().ToList();
            filterData.RequirementTypes = _zarnicaDb.RequirementTypes.OrderBy(m => m.Name).AsNoTracking().ToList();
            filterData.MilitaryUnits = _zarnicaDb.MilitaryUnits.OrderBy(m => m.Id).AsNoTracking().ToList();

            var person = _zarnicaDb.SpecialPersons
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .AsNoTracking()
                .FirstOrDefault(m => m.Id == id);
            filterData.DirectiveTypes = _zarnicaDb.DirectivesTypes
                .Where(m => m.Id == person.Requirement.DirectiveTypeId).AsNoTracking().ToList();
            var viewPerson = new SpecialPersonViewModel
            {
                Id = person.Id,
                LastName = person.LastName,
                FirstName = person.FirstName,
                Patronymic = person.Patronymic,
                BirthYear = person.BirthYear.ToString(),
                MilitaryComissariatId = person.MilitaryComissariatCode,
                DirectiveTypeId = person.Requirement.DirectiveTypeId,
                RequirementTypeId = person.Requirement.RequirementTypeId,
                MilitaryUnitId = person.Requirement.MilitaryUnitCode,
            };
            var teamsSendDate = _zarnicaDb.Teams
                .Where(m => m.MilitaryUnitCode == person.Requirement.MilitaryUnitCode && m.SendDate != null)
                .AsNoTracking()
                .Select(m => m.SendDate.Value)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
            
            if (!string.IsNullOrEmpty(person.Notice))
            {
                var noticeArray = person.SendDateString.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                DateTime sendDate;
                switch (noticeArray.Length)
                {
                    case 0:
                        viewPerson.SendDate = "-";
                        break;
                    case 1:
                        if (DateTime.TryParse(noticeArray[0].Trim('.').Trim(',').Trim(), out sendDate))
                        {
                            viewPerson.SendDate = sendDate.ToShortDateString();
                        }
                        else
                        {
                            viewPerson.SendDate = "-";
                            viewPerson.Notice = string.Join(' ', noticeArray);
                        }

                        break;
                    default:

                        if (DateTime.TryParse(noticeArray[0].Trim('.').Trim(',').Trim(), out sendDate))
                        {
                            viewPerson.SendDate = sendDate.ToShortDateString();
                            var noticeList = noticeArray.ToList();
                            noticeList.RemoveAt(0);
                            viewPerson.Notice = string.Join(' ', noticeList);
                        }
                        else
                        {
                            viewPerson.SendDate = "-";
                            viewPerson.Notice = string.Join(' ', noticeArray);
                        }

                        break;
                }
            }

            ViewBag.SendDates = teamsSendDate.Select(m => new
            {
                Value = m.ToShortDateString(),
                Text = m.ToShortDateString()
            }).ToList();
            ViewBag.filterData = filterData;
            return PartialView("_EditModal", viewPerson);
        }

        [HttpPost]
        public IActionResult Edit(SpecialPersonViewModel model)
        {
            try
            {
                if (model.RequirementTypeId == 0)
                {
                    ModelState.AddModelError("RequirementTypeId", "Не выбрано требование");
                }

                if (!ModelState.IsValid)
                {
                    var errorList = ModelState.Where(m => m.Value.Errors.Count > 0).ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return new JsonResult(new {isSucceeded = false, errors = errorList});
                }

                var person = _zarnicaDb.SpecialPersons.Include(m => m.SpecialPersonToRequirements)
                    .ThenInclude(m => m.Requirement)
                    .FirstOrDefault(m => m.Id == model.Id);
                if (person == null)
                {
                    var errorList = new Dictionary<string, string[]>
                    {
                        {"Id", new[] {"Не удалось найти персональщика. Перезагрузите страницу"}}
                    };
                    return new JsonResult(new {isSucceeded = false, errors = errorList});
                }

                person.LastName = model.LastName;
                person.FirstName = model.FirstName;
                person.Patronymic = model.Patronymic;
                person.BirthYear = int.Parse(model.BirthYear);
                person.MilitaryComissariatCode = model.MilitaryComissariatId;

                if (person.Requirement.MilitaryUnitCode != model.MilitaryUnitId ||
                    person.Requirement.RequirementTypeId != model.RequirementTypeId)
                {
                    var oldRequirementsRelative = person.SpecialPersonToRequirements;
                    var editRequirementRelative = new SpecialPersonToRequirement();

                    if (oldRequirementsRelative.Count == 1)
                    {
                        editRequirementRelative = person.SpecialPersonToRequirements.First();
                    }
                    else
                    {
                        editRequirementRelative = person.SpecialPersonToRequirements.FirstOrDefault(m =>
                            m.RequirementId == person.Requirement.Id);
                    }

                    var selectRequirements = person.SpecialPersonToRequirements.FirstOrDefault(m =>
                        m.Requirement.MilitaryUnitCode == model.MilitaryUnitId &&
                        m.Requirement.DirectiveTypeId == model.DirectiveTypeId &&
                        m.Requirement.RequirementTypeId == model.RequirementTypeId);

                    person.Requirement.Amount =
                        _zarnicaDb.SpecialPersonToRequirements.Count(m => m.RequirementId == person.Requirement.Id);
                    _zarnicaDb.Requirements.Update(person.Requirement);
                    if (selectRequirements == null)
                    {
                        var searchRequirement = _zarnicaDb.Requirements.FirstOrDefault(m =>
                            m.MilitaryUnitCode == model.MilitaryUnitId &&
                            m.DirectiveTypeId == model.DirectiveTypeId &&
                            m.RequirementTypeId == model.RequirementTypeId);

                        if (searchRequirement == null)
                        {
                            var nextIndex = _zarnicaDb.Requirements
                                .Where(m => m.DirectiveTypeId == model.DirectiveTypeId)
                                .AsNoTracking()
                                .Select(m => int.Parse(m.DocumentNumber)).Max() + 1;
                            var newIndex = "";
                            if (nextIndex < 10) newIndex = "00" + nextIndex;
                            else if (nextIndex < 100) newIndex = "0" + nextIndex;
                            else if (nextIndex < 100) newIndex = nextIndex.ToString();

                            var armyTypeId = _zarnicaDb.Teams
                                .AsNoTracking()
                                .FirstOrDefault(m => m.MilitaryUnitCode == model.MilitaryUnitId)?.ArmyTypeId;
                            var newId = _zarnicaDb.Requirements.Select(m => m.Id).Max() + 1;
                            var newRequirement = new Requirement
                            {
                                Id = newId,
                                DocumentNumber = newIndex,
                                ArmyTypeCode = armyTypeId,
                                CreateDate = DateTime.Now.Date,
                                UpdateDate = DateTime.Now.Date,
                                DirectiveTypeId = model.DirectiveTypeId,
                                RequirementTypeId = model.RequirementTypeId,
                                MilitaryUnitCode = model.MilitaryUnitId,
                                Amount = 1,
                                NotInfo = 0,
                                Notice = "Отправка со СП РТ",
                                UserName = "sprtSITE"
                            };
                            _zarnicaDb.Requirements.Add(newRequirement);
                            _zarnicaDb.SaveChanges();
                            editRequirementRelative.RequirementId = newRequirement.Id;
                        }
                        else
                        {
                            editRequirementRelative.RequirementId = searchRequirement.Id;
                        }
                    }

                    _zarnicaDb.SpecialPersonToRequirements.Update(editRequirementRelative);
                }

                if (person.SendDateString != (model.SendDate + " " + model.Notice).Trim())
                {
                    person.Notice = $"Отправка {model.SendDate} {model.Notice}";
                }

                _zarnicaDb.SpecialPersons.Update(person);
                _zarnicaDb.SaveChanges();
                return new JsonResult(new {isSucceeded = true});
            }
            catch (Exception e)
            {
                var errorList = new Dictionary<string, string[]>
                {
                    {"Id", new[] {"Критическая ошибка при изменении персональщика. Обратитесь в ВЦшнику"}}
                };
                return new JsonResult(new {isSucceeded = false, errors = errorList});
            }
        }


        public IActionResult SearchForDuplicates()
        {
            return Content("В разработке");
        }

        public IActionResult DeleteSpecialPerson()
        {
            return Content("В разработке");
        }
    }
}