using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using appModels = NewSprt.Data.App.Models;
using NewSprt.Data.Zarnica;
using NewSprt.Data.Zarnica.Models;
using NewSprt.Models.Extensions;
using NewSprt.Models.Helper.Documents;
using NewSprt.ViewModels;
using NewSprt.ViewModels.SpecialGuidance;
using NewSprt.ViewModels.FormModels;
using Npgsql;
using MilitaryComissariat = NewSprt.Data.Zarnica.Models.MilitaryComissariat;

namespace NewSprt.Controllers
{
    /// <summary>
    /// Контроллер для работы с персональщиками
    /// </summary>
    [Authorize(Policy = appModels.Permission.PersonalGuidance)]
    public class PersonalGuidanceController : Controller
    {
        /// <summary>
        /// Context для работы с базой Зарницы
        /// </summary>
        private readonly ZarnicaDbContext _zarnicaDb;

        /// <summary>
        /// SQL запрос для добавления нового требования
        /// С помощью Entity Framework невозможно добавить в базу зарницы (PostgreSQL 7.2(ОЧЕНЬ старая версия) не поддерживается на должном уровне)
        /// объект с автогенерацией ключа, при указании ключа в ручную могут появиться ошибки.
        /// В 2020г. (при написании) решить эту проблему её не получилось.
        /// </summary>
        private const string InsertRequirementQuery =
            "INSERT INTO zapiski (id, num, r7012, data_v, data, dir_type, nach, vchast, p102, prim, username) VALUES (nextval(\'public.zapiski_id_seq\'::text), @DocumentNumber, @ArmyTypeCode, @CreateDate, @UpdateDate, @DirectiveTypeId, @RequirementTypeId, @MilitaryUnitCode, 1, \'Отправка со СП РТ\', @UserName)";

        /// <summary>
        /// SQL запрос для добавления нового персональщика (без прикрепленного требования)
        /// </summary>
        private const string InsertSpecialPersonQuery =
            "INSERT INTO gsp05_d (id, k101_g5, data_g5, username, p006_g5, p005_g5, r8012_g5, prim_g5, p007_g5) VALUES (nextval(\'public.gsp05_d_id_seq\'::text), @BirthYear, @UpdateDate, @UpdateUser, @FirstName, @LastName, @MilitaryComissariatCode, @Notice, @Patronymic)";

        public PersonalGuidanceController(ZarnicaDbContext zarnicaDb)
        {
            _zarnicaDb = zarnicaDb;
        }

        /// <summary>
        /// Страница с информацией о персональщиках по воинским частям
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var specialMilitaryUnits = new List<string>() {"5561"};
            var militaryComissariats = await _zarnicaDb.MilitaryComissariats
                .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                .OrderBy(m => m.ShortName).AsNoTracking().ToListAsync();
            var persons = await _zarnicaDb.SpecialPersons
                .Where(m => m.SpecialPersonToRequirements.Any(s =>
                    s.Requirement.DirectiveTypeId == DirectiveType.PersonalPerson ||
                    s.Requirement.DirectiveTypeId == DirectiveType.FamilyPerson ||
                    s.Requirement.DirectiveTypeId == DirectiveType.SelectedToTheMilitaryUnitPerson))
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.RequirementType)
                .Include(m => m.SpecialPersonToRecruits)
                .ThenInclude(m => m.Recruit)
                .ThenInclude(m => m.Events)
                .Include(m => m.MilitaryComissariat)
                .OrderBy(m => m.LastName).AsNoTracking().ToListAsync();
            var requirements = persons.Select(m => m.Requirement).ToList();
            var militaryUnits = requirements.Select(m => m.MilitaryUnitCode).Distinct().ToList();
            var allTeams = await _zarnicaDb.Teams.Where(m => militaryUnits.Contains(m.MilitaryUnitCode))
                .Include(m => m.MilitaryUnit)
                .Include(m => m.ArmyType)
                .Include(m => m.MilitaryDistrict)
                .OrderBy(m => m.MilitaryUnitCode).AsNoTracking().ToListAsync();
            var mainTeams = allTeams.Where(m => m.SendDate == null).ToList();

            var teamWithSpecialPersons = new List<TeamWithSpecialPerson>();
            foreach (var mainTeam in mainTeams)
            {
                var teamWithSpecialPerson = new TeamWithSpecialPerson();
                var teamPersons = persons.Where(m => m.Requirement.MilitaryUnitCode == mainTeam.MilitaryUnitCode)
                    .ToList();

                

                var childrenTeams = allTeams
                    .Where(m => m.MilitaryUnitCode == mainTeam.MilitaryUnitCode && m.Id != mainTeam.Id)
                    .OrderBy(m => m.SendDate).ToList();
                var childrenTeamsSendDates = childrenTeams.Select(m => m.SendDate).Distinct().ToList();

                teamWithSpecialPerson.MainTeam = mainTeam;
                
                foreach (var person in teamPersons.Where(person => person.Recruit != null))
                {
                    if (person.Recruit.LastEvent.EventCode == 125 ||
                        person.Recruit.LastEvent.EventCode == 120 ||
                        person.Recruit.LastEvent.EventCode == 106 ||
                        person.Recruit.LastEvent.EventCode == 107 ||
                        person.Recruit.LastEvent.EventCode == 106 ||
                        person.Recruit.LastEvent.EventCode == 124)
                        person.IsMark = true;
                    
                    if (person.Recruit.TeamId != null &&
                        childrenTeams.Select(m => m.Id).Contains(person.Recruit.TeamId.Value))
                    {
                        person.IsDelivered = true;
                    }
                }
                foreach (var tsd in childrenTeamsSendDates)
                {
                    if (tsd == null) continue;
                    var teamSendDate = tsd.Value.Date;
                    if (teamSendDate.DayOfYear + 1 < DateTime.Now.DayOfYear) continue;

                    var sendChildrenTeams = childrenTeams
                        .Where(m => m.SendDate != null && m.SendDate.Value.DayOfYear == teamSendDate.DayOfYear)
                        .ToList();

                    var childrenTeam = new ChildrenTeam
                    {
                        Title = teamSendDate.ToShortDateString(),
                        AllCount = sendChildrenTeams.Sum(m => m.Amount),
                        Persons = teamPersons.Where(m =>
                            m.SendDate != null && m.SendDate.Value.DayOfYear == teamSendDate.DayOfYear).ToList()
                    };
                    childrenTeam.PersonsCount = childrenTeam.Persons.Count;

                    var tempTeamPatronages = new List<PatronageTask>();
                    foreach (var chteam in sendChildrenTeams.Where(chteam =>
                        !string.IsNullOrEmpty(chteam.PatronageRelations)))
                    {
                        tempTeamPatronages.AddRange(chteam.PatronageRelations.Split(",").Select(m =>
                                new PatronageTask
                                {
                                    MilitaryComissariat = militaryComissariats.First(t => t.Id == m.Split("#")[0]),
                                    Count = int.Parse(m.Split("#")[1])
                                })
                            .ToList());
                    }

                    var patronages = tempTeamPatronages.Select(m => m.MilitaryComissariat)
                        .Distinct()
                        .Select(militaryComissariat => new PatronageTask
                        {
                            MilitaryComissariat = militaryComissariat,
                            Count = tempTeamPatronages.Where(m => m.MilitaryComissariat.Id == militaryComissariat.Id)
                                .Sum(m => m.Count)
                        })
                        .ToList();

                    childrenTeam.PatronageTasks = patronages;
                    childrenTeam.PatronageTasksCount = patronages.Sum(m => m.Count - childrenTeam.Persons.Count(
                        p => p.MilitaryComissariatCode ==
                             m.MilitaryComissariat.Id));

                    if (specialMilitaryUnits.Contains(mainTeam.MilitaryUnitCode))
                    {
                        var plans = _zarnicaDb.TeamDistrictDistributions
                            .Where(m => m.MilitaryComissariatCode != "-" &&
                                        mainTeam.MilitaryUnitCode == m.MilitaryUnitCode &&
                                        teamSendDate.DayOfYear == m.SendDate.DayOfYear &&
                                        m.Education == "-" && m.AllCount != null && m.AllCount != 0)
                            .AsNoTracking().ToList();
                        foreach (var plan in plans)
                        {
                            plan.AllCount -= childrenTeam.Persons.Count(m =>
                                m.MilitaryComissariatCode == plan.MilitaryComissariatCode);
                        }

                        childrenTeam.PatronageTasksCount +=
                            plans.Where(m => m.AllCount != null && m.AllCount.Value > 0).Sum(m => m.AllCount.Value);
                    }

                    childrenTeam.RemainCount =
                        childrenTeam.AllCount - childrenTeam.PersonsCount - childrenTeam.PatronageTasksCount;
                    teamWithSpecialPerson.ChildrenTeams.Add(childrenTeam);
                }

                var notDistriburedPersons = teamPersons.Where(m => m.SendDate == null).ToList();
                teamWithSpecialPerson.ChildrenTeams.Add(new ChildrenTeam
                {
                    Title = "Не распределенные",
                    Persons = notDistriburedPersons,
                    PersonsCount = notDistriburedPersons.Count
                });
                teamWithSpecialPerson.AllCount = teamWithSpecialPerson.ChildrenTeams.Sum(m => m.AllCount);
                teamWithSpecialPerson.PersonsCount = teamWithSpecialPerson.ChildrenTeams.Sum(m => m.PersonsCount);
                teamWithSpecialPerson.PatronageRecruitsCount =
                    teamWithSpecialPerson.ChildrenTeams.Sum(m => m.PatronageTasksCount);
                teamWithSpecialPerson.RemainCount =
                    teamWithSpecialPerson.AllCount - teamWithSpecialPerson.PersonsCount -
                    teamWithSpecialPerson.PatronageRecruitsCount;
                teamWithSpecialPersons.Add(teamWithSpecialPerson);
            }

            teamWithSpecialPersons = teamWithSpecialPersons.Where(m => m.PersonsCount > 0).ToList();
            ViewData["personsCount"] = persons.Count;
            return View(teamWithSpecialPersons);
        }

        /// <summary>
        /// Страница фильтрами и выводом всех персональщиков
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> List()
        {
            var requirements = _zarnicaDb.Requirements
                .Select(m => new {m.MilitaryUnit, m.RequirementType}).AsNoTracking().ToList();

            var filterData = new FilterDataViewModel
            {
                MilitaryComissariats = await _zarnicaDb.MilitaryComissariats
                    .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                    .OrderBy(m => m.ShortName).AsNoTracking().ToListAsync(),
                MilitaryUnits = requirements.Select(m => m.MilitaryUnit)
                    .Where(m => m.Name != null).Distinct().OrderBy(m => m.Id).ToList(),
                RequirementTypes = requirements.Select(m => m.RequirementType)
                    .Where(m => m.Name != null).Distinct().OrderBy(m => m.Name).ToList(),
                DirectiveTypes = await _zarnicaDb.DirectivesTypes.AsNoTracking().ToListAsync()
            };
            ViewBag.FilterData = filterData;
            ViewData["personsCount"] = await _zarnicaDb.SpecialPersons.AsNoTracking().CountAsync();
            return View();
        }

        /// <summary>
        /// Вывод таблица на странице Index
        /// </summary>
        /// <param name="militaryComissariatId">Военный комиссариат</param>
        /// <param name="directorTypeId">Тип директивного указания</param>
        /// <param name="requirementTypeId">Требование от</param>
        /// <param name="militaryUnitId">Воинская часть</param>
        /// <param name="search">Поиск по ФИО (разделение пробелом)</param>
        /// <param name="isMark">При true вывод только персональщиков с ошибками в датах отправки</param>
        /// <param name="isDmo"></param>
        /// <param name="page">Номер страницы</param>
        /// <param name="rows">Кол-во элементов на 1 странице</param>
        /// <param name="exitMode">Моментальный выход из функции</param>
        /// <returns></returns>
        public async Task<PartialViewResult> ListGrid(
            string militaryComissariatId = "",
            int directorTypeId = 0,
            int requirementTypeId = 0,
            string militaryUnitId = "",
            string search = "",
            bool isMark = false,
            bool isDmo = false,
            int page = 1,
            int rows = 10,
            bool exitMode = false)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            if (exitMode) return PartialView("_ListGrid", new List<SpecialPerson>());

            var query = _zarnicaDb.SpecialPersons
                .Include(m => m.MilitaryComissariat)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.DirectiveType)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.RequirementType)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.MilitaryUnit)
                .Where(m => m.SpecialPersonToRequirements.Any())
                .AsNoTracking().AsEnumerable();

            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                query = query.Where(m => m.MilitaryComissariatCode == militaryComissariatId);
            }

            if (directorTypeId != 0)
            {
                query = query.Where(m =>
                    m.SpecialPersonToRequirements.Any(r => r.Requirement.DirectiveTypeId == directorTypeId));
            }

            if (requirementTypeId != 0)
            {
                query = query.Where(m =>
                    m.SpecialPersonToRequirements.Any(r => r.Requirement.RequirementTypeId == requirementTypeId));
            }

            if (!string.IsNullOrEmpty(militaryUnitId))
            {
                query = query.Where(m =>
                    m.SpecialPersonToRequirements.Any(r => r.Requirement.MilitaryUnitCode == militaryUnitId));
            }

            var persons = query.OrderBy(m => m.LastName).ToList();
            
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
            foreach (var pers in from pers in persons
                let requirementTeams = allTeams.Where(m =>
                        m.SendDate != null && m.MilitaryUnitCode == pers.Requirement.MilitaryUnitCode)
                    .OrderBy(m => m.SendDate).ToList()
                let requirementTeamsSendDate = requirementTeams.Select(m => m.SendDate).Distinct().ToList()
                where pers.SendDate != null && !requirementTeamsSendDate.Select(m => m?.DayOfYear)
                    .Contains(pers.SendDate?.DayOfYear)
                select pers)
            {
                pers.IsMark = true;
            }


            if (isMark) persons = persons.Where(m => m.IsMark).ToList();
            if (isDmo) persons = persons.Where(m => m.IsDmo).ToList();

            return PartialView("_ListGrid", persons);
        }

        /// <summary>
        /// Модальное окно для добавления нового персональщика
        /// </summary>
        /// <returns></returns>
        public PartialViewResult CreateModal()
        {
            var filterData = new FilterDataViewModel
            {
                MilitaryComissariats = _zarnicaDb.MilitaryComissariats
                    .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                    .OrderBy(m => m.ShortName).AsNoTracking().ToList(),
                RequirementTypes = _zarnicaDb.RequirementTypes.OrderBy(m => m.Name).AsNoTracking().ToList(),
                MilitaryUnits = _zarnicaDb.MilitaryUnits.OrderBy(m => m.Id).AsNoTracking().ToList(),
                DirectiveTypes = _zarnicaDb.DirectivesTypes.OrderBy(m => m.Id).AsNoTracking().ToList()
            };
            ViewBag.filterData = filterData;
            ViewBag.SendDates = new List<object>();
            var viewPerson = new SpecialPersonViewModel
            {
                DirectiveTypeId = DirectiveType.PersonalPerson,
                RequirementTypeId = RequirementType.VkrtRequirement
            };
            return PartialView("_CreateModal", viewPerson);
        }

        /// <summary>
        /// Добавление нового персональщика в базу Заринцы
        /// </summary>
        /// <param name="model">ViewModel для проверки валидации</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public IActionResult Create(SpecialPersonViewModel model)
        {
            if (model.RequirementTypeId == 0)
            {
                ModelState.AddModelError("RequirementTypeId", "Не выбрано требование");
            }

            var transaction = _zarnicaDb.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
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
                    return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
                }


                var newPerson = new SpecialPerson
                {
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    Patronymic = model.Patronymic,
                    BirthYear = int.Parse(model.BirthYear),
                    MilitaryComissariatCode = model.MilitaryComissariatId,
                    Notice = (model.SendDate == null ? "" : $"Отправка {model.SendDate} ") + (model.IsDmo ? "ДМО " : "") + model.Notice,
                    UpdateDate = DateTime.Now.Date,
                    UpdateUser = User.Identity.Name.ToLower()
                };

                _zarnicaDb.Database.ExecuteSqlCommand(InsertSpecialPersonQuery,
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
                    .Include(m => m.SpecialPersonsInRequirement)
                    .FirstOrDefault(m => m.DirectiveTypeId == model.DirectiveTypeId &&
                                         m.RequirementTypeId == model.RequirementTypeId &&
                                         m.MilitaryUnitCode == model.MilitaryUnitId);
                if (requirement == null)
                {
                    var nextIndex = _zarnicaDb.Requirements
                        .Where(m => m.DirectiveTypeId == model.DirectiveTypeId)
                        .AsNoTracking()
                        .Select(m => int.Parse(m.DocumentNumber)).Max() + 1;
                    var newIndex = nextIndex.ToString();
                    if (nextIndex < 10) newIndex = "00" + nextIndex;
                    else if (nextIndex < 100) newIndex = "0" + nextIndex;

                    var armyTypeId = _zarnicaDb.Teams.AsNoTracking()
                        .FirstOrDefault(m => m.MilitaryUnitCode == model.MilitaryUnitId)?.ArmyTypeId;

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

                    _zarnicaDb.Database.ExecuteSqlCommand(InsertRequirementQuery,
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
                    requirement.Amount += requirement.SpecialPersonsInRequirement.Count;
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
                if (requirement == null || newPerson == null) throw new NullReferenceException();
                _zarnicaDb.SpecialPersonToRequirements.Add(new SpecialPersonToRequirement
                    {RequirementId = requirement.Id, SpecialPersonId = newPerson.Id});
                _zarnicaDb.SaveChanges();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id", "Критическая ошибка при изменении персональщика. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        /// <summary>
        /// Модальное окно для изменения персональщика
        /// </summary>
        /// <param name="id">Id персональщика в Зарнице</param>
        /// <returns></returns>
        public PartialViewResult EditModal(int id)
        {
            var person = _zarnicaDb.SpecialPersons
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .AsNoTracking()
                .FirstOrDefault(m => m.Id == id);
            if (person == null) return null;
            var filterData = new FilterDataViewModel
            {
                MilitaryComissariats = _zarnicaDb.MilitaryComissariats
                    .Where(m => m.Region == MilitaryComissariat.CurrentRegion && m.IsEmpty == "0")
                    .OrderBy(m => m.ShortName).AsNoTracking().ToList(),
                RequirementTypes = _zarnicaDb.RequirementTypes.OrderBy(m => m.Name).AsNoTracking().ToList(),
                MilitaryUnits = _zarnicaDb.MilitaryUnits.OrderBy(m => m.Id).AsNoTracking().ToList(),
                DirectiveTypes = _zarnicaDb.DirectivesTypes.OrderBy(m => m.Id).AsNoTracking().ToList()
            };


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
                IsDmo = person.IsDmo
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
                var noticeArray = person.SendDateString.Replace("ДМО", "" , StringComparison.OrdinalIgnoreCase)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
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

        /// <summary>
        /// Изменение персональщика
        /// </summary>
        /// <param name="model">ViewModel для проверки валидации</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        [HttpPost]
        public IActionResult Edit(SpecialPersonViewModel model)
        {
            if (model.RequirementTypeId == 0)
            {
                ModelState.AddModelError("RequirementTypeId", "Не выбрано требование");
            }

            if (!ModelState.IsValid)
            {
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }

            var transaction = _zarnicaDb.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var person = _zarnicaDb.SpecialPersons.Include(m => m.SpecialPersonToRequirements)
                    .ThenInclude(m => m.Requirement)
                    .FirstOrDefault(m => m.Id == model.Id);
                if (person == null)
                {
                    ModelState.AddModelError("Id", "Не удалось найти персональщика. Обновите страницу");
                    return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
                }

                person.LastName = model.LastName;
                person.FirstName = model.FirstName;
                person.Patronymic = model.Patronymic;
                person.BirthYear = int.Parse(model.BirthYear);
                person.MilitaryComissariatCode = model.MilitaryComissariatId;

                if (person.Requirement.MilitaryUnitCode != model.MilitaryUnitId ||
                    person.Requirement.RequirementTypeId != model.RequirementTypeId ||
                    person.Requirement.DirectiveTypeId != model.DirectiveTypeId)
                {
                    var oldRequirementsRelative = person.SpecialPersonToRequirements;
                    SpecialPersonToRequirement deleteRequirementRelative;

                    if (oldRequirementsRelative.Count == 1)
                    {
                        deleteRequirementRelative = person.SpecialPersonToRequirements.First();
                    }
                    else
                    {
                        deleteRequirementRelative = person.SpecialPersonToRequirements.FirstOrDefault(m =>
                            m.RequirementId == person.Requirement.Id);
                    }

                    if (deleteRequirementRelative == null) throw new NullReferenceException();
                    var selectRequirements = person.SpecialPersonToRequirements.FirstOrDefault(m =>
                        m.Requirement.MilitaryUnitCode == model.MilitaryUnitId &&
                        m.Requirement.DirectiveTypeId == model.DirectiveTypeId &&
                        m.Requirement.RequirementTypeId == model.RequirementTypeId);

                    person.Requirement.Amount =
                        _zarnicaDb.SpecialPersonToRequirements.Count(m => m.RequirementId == person.Requirement.Id);
                    _zarnicaDb.Requirements.Update(person.Requirement);
                    _zarnicaDb.SpecialPersonToRequirements.Remove(deleteRequirementRelative);
                    var newRequirementRelative = new SpecialPersonToRequirement {SpecialPersonId = person.Id};
                    if (selectRequirements == null)
                    {
                        var searchRequirement = _zarnicaDb.Requirements
                            .Include(m => m.SpecialPersonsInRequirement)
                            .FirstOrDefault(m =>
                                m.MilitaryUnitCode == model.MilitaryUnitId &&
                                m.DirectiveTypeId == model.DirectiveTypeId &&
                                m.RequirementTypeId == model.RequirementTypeId);

                        if (searchRequirement == null)
                        {
                            var nextIndex = _zarnicaDb.Requirements
                                .Where(m => m.DirectiveTypeId == model.DirectiveTypeId)
                                .AsNoTracking()
                                .Select(m => int.Parse(m.DocumentNumber)).Max() + 1;
                            var newIndex = nextIndex.ToString();
                            if (nextIndex < 10) newIndex = "00" + nextIndex;
                            else if (nextIndex < 100) newIndex = "0" + nextIndex;

                            var armyTypeId = _zarnicaDb.Teams
                                .AsNoTracking()
                                .FirstOrDefault(m => m.MilitaryUnitCode == model.MilitaryUnitId)?.ArmyTypeId;

                            var requirement = new Requirement
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

                            _zarnicaDb.Database.ExecuteSqlCommand(InsertRequirementQuery,
                                new NpgsqlParameter("DocumentNumber", requirement.DocumentNumber),
                                new NpgsqlParameter("ArmyTypeCode", requirement.ArmyTypeCode),
                                new NpgsqlParameter("CreateDate", requirement.CreateDate.Date),
                                new NpgsqlParameter("UpdateDate", requirement.UpdateDate.Value.Date),
                                new NpgsqlParameter("DirectiveTypeId", requirement.DirectiveTypeId),
                                new NpgsqlParameter("RequirementTypeId", requirement.RequirementTypeId),
                                new NpgsqlParameter("MilitaryUnitCode", requirement.MilitaryUnitCode),
                                new NpgsqlParameter("UserName", requirement.UserName));
                            requirement = _zarnicaDb.Requirements.AsNoTracking()
                                .FirstOrDefault(m => m.DirectiveTypeId == model.DirectiveTypeId &&
                                                     m.RequirementTypeId == model.RequirementTypeId &&
                                                     m.MilitaryUnitCode == model.MilitaryUnitId);
                            if (requirement == null) throw new NullReferenceException();
                            newRequirementRelative.RequirementId = requirement.Id;
                        }
                        else
                        {
                            newRequirementRelative.RequirementId = searchRequirement.Id;
                            searchRequirement.Amount = searchRequirement.SpecialPersonsInRequirement.Count + 1;
                            _zarnicaDb.Requirements.Update(searchRequirement);
                        }
                    }

                    _zarnicaDb.SpecialPersonToRequirements.Add(newRequirementRelative);
                }

                if (person.SendDateString != (model.SendDate == null ? "" : $"Отправка {model.SendDate} ") + (model.IsDmo ? "ДМО " : "") + model.Notice)
                {
                    person.Notice = (model.SendDate == null ? "" : $"Отправка {model.SendDate} ") + (model.IsDmo ? "ДМО " : "") + model.Notice;
                    person.Notice = person.Notice.Trim();
                }
                
                _zarnicaDb.SpecialPersons.Update(person);
                _zarnicaDb.SaveChanges();
                transaction.Commit();
                return new JsonResult(new {isSucceeded = true});
            }
            catch
            {
                transaction.Rollback();
                ModelState.AddModelError("Id", "Критическая ошибка при изменении персональщика. Обратитесь в ВЦшнику");
                return new JsonResult(new {isSucceeded = false, errors = ModelState.Errors()});
            }
        }

        /// <summary>
        /// Страница поиска персональщиков с двумя и более требованиями
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SearchForDuplicates()
        {
            var persons = await _zarnicaDb.SpecialPersons
                .Where(m => m.SpecialPersonToRequirements.Count > 1)
                .Include(m => m.MilitaryComissariat)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.RequirementType)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.MilitaryUnit)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.DirectiveType)
                .AsNoTracking().ToListAsync();
            ViewData["duplicatesCount"] = persons.Count;
            if (!HttpContext.Session.Keys.Contains("alert")) return View(persons);
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View(persons);
        }

        /// <summary>
        /// Удаление связи между персональщиком и требованием
        /// </summary>
        /// <param name="requirementId">Id требования</param>
        /// <param name="personId">Id персональщика</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteRequirementFromPerson(int requirementId, int personId)
        {
            var transaction = await _zarnicaDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var specialPersonToRequirement = await _zarnicaDb.SpecialPersonToRequirements
                    .Include(m => m.Requirement)
                    .ThenInclude(m => m.SpecialPersonsInRequirement)
                    .FirstOrDefaultAsync(m =>
                        m.RequirementId == requirementId && m.SpecialPersonId == personId);
                if (specialPersonToRequirement == null)
                {
                    HttpContext.Session.Set("alert",
                        new AlertViewModel(AlertType.Error,
                            "Требование не найдено!"));
                    return RedirectToAction("SearchForDuplicates");
                }

                specialPersonToRequirement.Requirement.Amount =
                    specialPersonToRequirement.Requirement.SpecialPersonsInRequirement.Count - 1;
                _zarnicaDb.Requirements.Update(specialPersonToRequirement.Requirement);
                _zarnicaDb.SpecialPersonToRequirements.Remove(specialPersonToRequirement);
                await _zarnicaDb.SaveChangesAsync();
                transaction.Commit();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Success, "Требование персональщика успешно удалено!"));
            }
            catch
            {
                transaction.Rollback();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error, "Ошибка при удалении требования!"));
            }
            
            return RedirectToAction("SearchForDuplicates");
        }

        /// <summary>
        /// Страница удаления убывших персональщиков
        /// </summary>
        /// <returns></returns>
        public IActionResult RemovingTheDepartingSpecialPerson()
        {
            if (!HttpContext.Session.Keys.Contains("alert")) return View();
            ViewBag.Alert = HttpContext.Session.Get<AlertViewModel>("alert");
            HttpContext.Session.Remove("alert");
            return View();
        }

        public async Task<PartialViewResult> RemovingTheDepartingSpecialPersonGrid(int page = 1, int rows = 10)
        {
            ViewBag.Pagination = new Pagination(rows, page);
            var persons = await _zarnicaDb.SpecialPersons
                .Include(m => m.MilitaryComissariat)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.RequirementType)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.MilitaryUnit)
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.DirectiveType)
                .Include(m => m.SpecialPersonToRecruits)
                .ThenInclude(m => m.Recruit)
                .ThenInclude(m => m.Team)
                .ThenInclude(m => m.MilitaryUnit)
                .Include(m => m.SpecialPersonToRecruits)
                .ThenInclude(m => m.Recruit)
                .ThenInclude(m => m.Events)
                .AsNoTracking()
                .ToListAsync();

            persons = persons.Where(m => (m.SendDate != null && m.SendDate.Value.DayOfYear < DateTime.Now.DayOfYear) ||
                                         (m.Recruit?.Team?.SendDate != null 
                                          && m.Recruit.Team.SendDate.Value.DayOfYear < DateTime.Now.DayOfYear))
                .ToList();
            foreach (var person in persons
                .Where(person => person.Recruit?.Team != null &&
                                 person.Recruit.Team.MilitaryUnitCode == person.Requirement.MilitaryUnitCode))
            {
                person.IsMark = true;
            }

            return PartialView("_RemovingTheDepartingSpecialPersonGrid", persons);
        }

        /// <summary>
        /// Удаление персональщика по ID
        /// </summary>
        /// <param name="id">Id персональщика</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteSpecialPerson(int id)
        {
            var transaction = await _zarnicaDb.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var person = await _zarnicaDb.SpecialPersons
                    .Include(m => m.SpecialPersonToRecruits)
                    .Include(m => m.SpecialPersonToRequirements)
                    .ThenInclude(m => m.Requirement)
                    .ThenInclude(m => m.SpecialPersonsInRequirement)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (person == null)
                {
                    HttpContext.Session.Set("alert",
                        new AlertViewModel(AlertType.Error,
                            "Персональщик не найден!"));
                    return RedirectToAction("RemovingTheDepartingSpecialPerson");
                }

                if (person.SpecialPersonToRequirements.Any())
                {
                    foreach (var requirement in person.SpecialPersonToRequirements.Select(m => m.Requirement))
                    {
                        requirement.Amount = requirement.SpecialPersonsInRequirement.Count - 1;
                    }

                    _zarnicaDb.Requirements.UpdateRange(person.SpecialPersonToRequirements.Select(m => m.Requirement));

                    _zarnicaDb.SpecialPersonToRequirements.RemoveRange(person.SpecialPersonToRequirements);
                }

                _zarnicaDb.SpecialPersonToRecruits.RemoveRange(person.SpecialPersonToRecruits);
                await _zarnicaDb.SaveChangesAsync();
                _zarnicaDb.SpecialPersons.Remove(person);
                await _zarnicaDb.SaveChangesAsync();

                transaction.Commit();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Success, "Персональщик успешно удален!"));
                return RedirectToAction("RemovingTheDepartingSpecialPerson");
            }
            catch
            {
                transaction.Rollback();
                HttpContext.Session.Set("alert",
                    new AlertViewModel(AlertType.Error, "Ошибка при удалении персональщика!"));
                return RedirectToAction("RemovingTheDepartingSpecialPerson");
            }
        }

        public async Task<IActionResult> PrintPersonalGuidanceReport(
            string militaryComissariatId = "",
            int directiveTypeId = 0)
        {
            var qPersons = _zarnicaDb.SpecialPersons
                .Include(m => m.SpecialPersonToRequirements)
                .ThenInclude(m => m.Requirement)
                .ThenInclude(m => m.MilitaryUnit).AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                qPersons = qPersons.Where(m => m.MilitaryComissariatCode == militaryComissariatId);
            }

            if (directiveTypeId != 0)
            {
                qPersons = qPersons.Where(m => m.Requirement.DirectiveTypeId == directiveTypeId);
            }

            var persons = await qPersons.OrderBy(m => m.LastName).ToListAsync();

            if (!string.IsNullOrEmpty(militaryComissariatId))
            {
                var militaryComissariat = await _zarnicaDb.MilitaryComissariats.FirstOrDefaultAsync(m => m.Id == militaryComissariatId);
                return File(WordDocumentHelper.GeneratePersonalGuidanceReport(persons, militaryComissariat.Name),
                    WordDocumentHelper.OutputFormatType,
                    $"{militaryComissariat.Name}.docx");
            }

            return RedirectToAction("List");
        }
    }
}