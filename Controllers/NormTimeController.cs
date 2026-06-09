using System.Globalization;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.AdditionalWork;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.ViewModels.NormTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class NormTimeController : Controller
    {
        private const string PracticeCategoryName = "Практика";
        private const string ResearchCategoryName = "Научная работа";
        private const string GiaCategoryName = "ГИА";

        private const string StateExamConsultationNormName = "Консультации к госэкзамену";
        private const string OldStateExamConsultationNormName = "Консультация к госэкзамену";

        private const string PostgraduateNormCode = "POSTGRADUATE_SUPERVISION";
        private const string DepartmentHeadNormCode = "DEPARTMENT_HEAD";
        private const string DeputyDeanAcademicNormCode = "DEPUTY_DEAN_ACADEMIC";
        private const string DeputyDeanEducationalNormCode = "DEPUTY_DEAN_EDUCATIONAL";

        private static readonly string[] AdditionalWorkCodes =
        {
            PostgraduateNormCode,
            DepartmentHeadNormCode,
            DeputyDeanAcademicNormCode,
            DeputyDeanEducationalNormCode
        };

        private readonly DepartmentLoadDbContext _context;

        public NormTimeController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? startYear, string? activeTab = null)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            await EnsureDefaultAdditionalWorkNormsAsync();
            await EnsureDefaultGiaNormsAsync();
            await EnsurePracticeNormTimesFromCalculationRowsAsync(selectedYear);

            var model = await BuildPageModelAsync(
                selectedYearStart,
                selectedYear,
                activeTab);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int? startYear, string? activeTab)
        {
            var selectedYearStart = AcademicYearResolver.NormalizeStartYear(startYear);
            var selectedYear = AcademicYearResolver.BuildAcademicYear(selectedYearStart);

            await EnsureDefaultAdditionalWorkNormsAsync();
            await EnsureDefaultGiaNormsAsync();
            await EnsurePracticeNormTimesFromCalculationRowsAsync(selectedYear);

            await SaveMainNormTimesFromFormAsync();
            await SaveAdditionalWorkNormsFromFormAsync();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Нормы времени сохранены";

            return RedirectToAction(nameof(Index), new
            {
                startYear = selectedYearStart,
                activeTab = NormalizeActiveTab(activeTab)
            });
        }

        private async Task SaveMainNormTimesFromFormAsync()
        {
            var form = Request.Form;
            var ids = ReadIndexedIntValues(form, "Items", "Id");

            if (ids.Count == 0)
            {
                return;
            }

            var dbItems = await _context.NormTimes
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var dbItem in dbItems)
            {
                var index = FindIndexById(form, "Items", dbItem.Id);

                if (index < 0)
                {
                    continue;
                }

                var calculationBaseValue = form[$"Items[{index}].CalculationBase"].ToString();

                if (int.TryParse(calculationBaseValue, out var calculationBaseInt)
                    && Enum.IsDefined(typeof(WorkCalculationBase), calculationBaseInt))
                {
                    dbItem.CalculationBase = (WorkCalculationBase)calculationBaseInt;
                }

                var hours = ReadDecimalFromForm(form[$"Items[{index}].Hours"].ToString());

                dbItem.Hours = Math.Round(
                    Math.Max(0, hours),
                    2,
                    MidpointRounding.AwayFromZero);

                var weeksCount = ReadIntFromForm(form[$"Items[{index}].WeeksCount"].ToString());
                dbItem.WeeksCount = Math.Max(0, weeksCount);
            }
        }

        private async Task SaveAdditionalWorkNormsFromFormAsync()
        {
            var form = Request.Form;

            var ids = ReadIndexedIntValues(form, "AdditionalWorkNorms", "Id");

            if (ids.Count == 0)
            {
                return;
            }

            var dbItems = await _context.AdditionalWorkNorms
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            foreach (var dbItem in dbItems)
            {
                var index = FindIndexById(form, "AdditionalWorkNorms", dbItem.Id);

                if (index < 0)
                {
                    continue;
                }

                var count = ReadIntFromForm(
                    form[$"AdditionalWorkNorms[{index}].Count"].ToString());

                var hours = ReadDecimalFromForm(
                    form[$"AdditionalWorkNorms[{index}].Hours"].ToString());

                dbItem.Count = dbItem.WorkType == AdditionalWorkType.PostgraduateSupervision
                    ? Math.Max(0, count)
                    : 1;

                dbItem.Hours = Math.Round(
                    Math.Max(0, hours),
                    2,
                    MidpointRounding.AwayFromZero);

                dbItem.IsDefault = false;
            }
        }
        private async Task<NormTimePageViewModel> BuildPageModelAsync(
            int selectedYearStart,
            string selectedYear,
            string? activeTab)
        {
            var practiceNameKeys = (await GetPracticeNamesForYearAsync(selectedYear))
                .Select(NormalizeKey)
                .ToHashSet();

            var items = await _context.NormTimes
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.WorkName)
                .ToListAsync();

            items = items
                .Where(x => ShouldShowNormTime(x, practiceNameKeys))
                .ToList();

            var additionalItems = await _context.AdditionalWorkNorms
                .AsNoTracking()
                .Where(x => AdditionalWorkCodes.Contains(x.Code))
                .ToListAsync();

            additionalItems = additionalItems
                .OrderBy(x => GetAdditionalWorkSortOrder(x.Code))
                .ToList();

            return new NormTimePageViewModel
            {
                SelectedYearStart = selectedYearStart,
                SelectedYear = selectedYear,
                AvailableYearStarts = AcademicYearResolver.BuildAvailableStartYears(selectedYearStart),
                Items = items
                    .Select(x => new NormTimeRowViewModel
                    {
                        Id = x.Id,
                        WorkName = x.WorkName,
                        CategoryName = x.CategoryName,
                        CalculationBase = x.CalculationBase,
                        Hours = x.Hours,
                        WeeksCount = x.WeeksCount
                    })
                    .ToList(),
                AdditionalWorkNorms = additionalItems
                    .Select(x => new AdditionalWorkNormRowViewModel
                    {
                        Id = x.Id,
                        WorkType = x.WorkType,
                        Code = x.Code,
                        Name = x.Name,
                        Count = x.Count,
                        Hours = x.Hours
                    })
                    .ToList(),
                ActiveTab = NormalizeActiveTab(activeTab)
            };
        }

        private async Task EnsureDefaultGiaNormsAsync()
        {
            var norms = await _context.NormTimes.ToListAsync();

            var oldConsultationNorm = norms.FirstOrDefault(x =>
                NormalizeKey(x.CategoryName) == NormalizeKey(GiaCategoryName)
                && NormalizeKey(x.WorkName) == NormalizeKey(OldStateExamConsultationNormName));

            var newConsultationNorm = norms.FirstOrDefault(x =>
                NormalizeKey(x.CategoryName) == NormalizeKey(GiaCategoryName)
                && NormalizeKey(x.WorkName) == NormalizeKey(StateExamConsultationNormName));

            if (newConsultationNorm != null)
            {
                newConsultationNorm.WorkName = StateExamConsultationNormName;
                newConsultationNorm.CategoryName = GiaCategoryName;
                return;
            }

            if (oldConsultationNorm != null)
            {
                oldConsultationNorm.WorkName = StateExamConsultationNormName;
                oldConsultationNorm.CategoryName = GiaCategoryName;
                return;
            }

            var maxSortOrder = norms.Count == 0
                ? 0
                : norms.Max(x => x.SortOrder);

            _context.NormTimes.Add(new NormTime
            {
                WorkName = StateExamConsultationNormName,
                CategoryName = GiaCategoryName,
                CalculationBase = WorkCalculationBase.PerWork,
                Hours = 0m,
                WeeksCount = 0,
                SortOrder = maxSortOrder + 1
            });

            await _context.SaveChangesAsync();
        }

        private async Task EnsurePracticeNormTimesFromCalculationRowsAsync(string selectedYear)
        {
            var practiceNames = await GetPracticeNamesForYearAsync(selectedYear);

            if (practiceNames.Count == 0)
            {
                return;
            }

            var existingNorms = await _context.NormTimes.ToListAsync();

            var existingKeys = existingNorms
                .Select(x => NormalizeKey(x.WorkName))
                .ToHashSet();

            var maxSortOrder = existingNorms.Count == 0
                ? 0
                : existingNorms.Max(x => x.SortOrder);

            var hasChanges = false;

            foreach (var practiceName in practiceNames)
            {
                var key = NormalizeKey(practiceName);

                if (existingKeys.Contains(key))
                {
                    continue;
                }

                var similarNorm = FindSimilarPracticeNorm(existingNorms, practiceName);

                _context.NormTimes.Add(new NormTime
                {
                    WorkName = practiceName.Trim(),
                    CategoryName = ResolvePracticeCategory(practiceName),
                    CalculationBase = similarNorm?.CalculationBase ?? WorkCalculationBase.PerStudent,
                    Hours = similarNorm?.Hours ?? 0m,
                    WeeksCount = similarNorm?.WeeksCount ?? 0,
                    SortOrder = ++maxSortOrder
                });

                existingKeys.Add(key);
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<string>> GetPracticeNamesForYearAsync(string selectedYear)
        {
            var rawNames = await _context.PracticeWorkloadRows
                .AsNoTracking()
                .Where(x => x.PlanYear == selectedYear)
                .Select(x => x.PracticeName)
                .ToListAsync();

            return rawNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .GroupBy(NormalizeKey)
                .Select(x => x.First())
                .OrderBy(x => x)
                .ToList();
        }

        private static bool ShouldShowNormTime(
            NormTime item,
            HashSet<string> practiceNameKeys)
        {
            if (!IsPracticeOrResearchCategory(item.CategoryName))
            {
                return true;
            }

            return practiceNameKeys.Contains(NormalizeKey(item.WorkName));
        }

        private static NormTime? FindSimilarPracticeNorm(
            List<NormTime> existingNorms,
            string practiceName)
        {
            return existingNorms
                .Where(x => IsPracticeOrResearchCategory(x.CategoryName))
                .FirstOrDefault(x => IsSamePracticeType(x.WorkName, practiceName));
        }

        private static bool IsPracticeOrResearchCategory(string? categoryName)
        {
            return ContainsNormalized(categoryName, "практи")
                   || ContainsNormalized(categoryName, "науч");
        }

        private static string ResolvePracticeCategory(string practiceName)
        {
            return IsResearchPractice(practiceName)
                ? ResearchCategoryName
                : PracticeCategoryName;
        }

        private static bool IsResearchPractice(string? practiceName)
        {
            var normalized = NormalizeKey(practiceName);

            return normalized.Contains("научно исследователь")
                   || normalized.Contains("научно-исследователь")
                   || normalized == "нир"
                   || normalized == "ниrм";
        }

        private static bool IsSamePracticeType(string? left, string? right)
        {
            var a = NormalizePracticeTypeKey(left);
            var b = NormalizePracticeTypeKey(right);

            if (a == b)
            {
                return true;
            }

            if (a.Contains("технологическ") && b.Contains("технологическ"))
            {
                return true;
            }

            if (a.Contains("преддиплом") && b.Contains("преддиплом"))
            {
                return true;
            }

            if (a.Contains("ознаком") && b.Contains("ознаком"))
            {
                return true;
            }

            if (a.Contains("научно исследователь") && b.Contains("научно исследователь"))
            {
                return true;
            }

            if (a.Contains("учебн") && b.Contains("учебн"))
            {
                return true;
            }

            return false;
        }

        private static string NormalizePracticeTypeKey(string? value)
        {
            return NormalizeKey(value)
                .Replace("бакалавров", string.Empty)
                .Replace("магистров", string.Empty)
                .Replace("учебная", "учебн")
                .Replace("учебной", "учебн")
                .Trim();
        }

        private async Task EnsureDefaultAdditionalWorkNormsAsync()
        {
            var existingItems = await _context.AdditionalWorkNorms
                .ToListAsync();

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.PostgraduateSupervision,
                PostgraduateNormCode,
                "Руководство аспирантами (за год), часы на человека");

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.OrganizationalWork,
                DepartmentHeadNormCode,
                "Руководство кафедрой");

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.OrganizationalWork,
                DeputyDeanAcademicNormCode,
                "Зам декана по учебной работе");

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.OrganizationalWork,
                DeputyDeanEducationalNormCode,
                "Зам декана по воспит.работе");

            await _context.SaveChangesAsync();
        }

        private void AddOrUpdateNorm(
       List<AdditionalWorkNorm> existingItems,
       AdditionalWorkType workType,
       string code,
       string name)
        {
            var existingItem = existingItems
                .FirstOrDefault(x => x.Code == code);

            if (existingItem == null)
            {
                _context.AdditionalWorkNorms.Add(new AdditionalWorkNorm
                {
                    WorkType = workType,
                    Code = code,
                    Name = name,
                    Count = workType == AdditionalWorkType.PostgraduateSupervision ? 0 : 1,
                    Hours = 0m,
                    IsDefault = false
                });

                return;
            }

            existingItem.WorkType = workType;
            existingItem.Name = name;
            existingItem.Count = workType == AdditionalWorkType.PostgraduateSupervision
                ? Math.Max(0, existingItem.Count)
                : 1;
            existingItem.IsDefault = false;
        }
        private static List<int> ReadIndexedIntValues(
            IFormCollection form,
            string collectionName,
            string propertyName)
        {
            var result = new List<int>();

            foreach (var key in form.Keys)
            {
                if (!key.StartsWith($"{collectionName}[", StringComparison.OrdinalIgnoreCase)
                    || !key.EndsWith($"].{propertyName}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (int.TryParse(form[key].ToString(), out var value))
                {
                    result.Add(value);
                }
            }

            return result;
        }

        private static int FindIndexById(
            IFormCollection form,
            string collectionName,
            int id)
        {
            foreach (var key in form.Keys)
            {
                if (!key.StartsWith($"{collectionName}[", StringComparison.OrdinalIgnoreCase)
                    || !key.EndsWith("].Id", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = form[key].ToString();

                if (!int.TryParse(value, out var parsedId) || parsedId != id)
                {
                    continue;
                }

                var start = key.IndexOf('[', StringComparison.Ordinal);
                var end = key.IndexOf(']', StringComparison.Ordinal);

                if (start < 0 || end <= start)
                {
                    continue;
                }

                var indexText = key.Substring(start + 1, end - start - 1);

                if (int.TryParse(indexText, out var index))
                {
                    return index;
                }
            }

            return -1;
        }

        private static decimal ReadDecimalFromForm(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0m;
            }

            var normalized = value.Trim();

            if (normalized.Contains(',') && !normalized.Contains('.'))
            {
                if (decimal.TryParse(
                        normalized,
                        NumberStyles.Number,
                        CultureInfo.GetCultureInfo("ru-RU"),
                        out var ruCommaValue))
                {
                    return ruCommaValue;
                }
            }

            if (decimal.TryParse(
                    normalized,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var invariantValue))
            {
                return invariantValue;
            }

            if (decimal.TryParse(
                    normalized,
                    NumberStyles.Number,
                    CultureInfo.GetCultureInfo("ru-RU"),
                    out var ruValue))
            {
                return ruValue;
            }

            normalized = normalized.Replace(',', '.');

            if (decimal.TryParse(
                    normalized,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var replacedValue))
            {
                return replacedValue;
            }

            return 0m;
        }

        private static int ReadIntFromForm(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            return int.TryParse(value.Trim(), out var result)
                ? result
                : 0;
        }

        private static int GetAdditionalWorkSortOrder(string code)
        {
            return code switch
            {
                PostgraduateNormCode => 1,
                DepartmentHeadNormCode => 2,
                DeputyDeanAcademicNormCode => 3,
                DeputyDeanEducationalNormCode => 4,
                _ => 100
            };
        }

        private static string NormalizeActiveTab(string? activeTab)
        {
            return string.IsNullOrWhiteSpace(activeTab)
                ? string.Empty
                : activeTab.Trim();
        }

        private static string NormalizeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value
                .Trim()
                .ToLowerInvariant()
                .Replace("ё", "е")
                .Replace("-", " ");

            return string.Join(
                ' ',
                normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private static bool ContainsNormalized(string? value, string fragment)
        {
            return NormalizeKey(value).Contains(NormalizeKey(fragment));
        }
    }
}