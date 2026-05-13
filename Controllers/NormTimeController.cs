using System.Globalization;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models.AdditionalWork;
using DepartmentLoadApp.Models.Enums;
using DepartmentLoadApp.ViewModels.NormTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class NormTimeController : Controller
    {
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
        public async Task<IActionResult> Index(string? activeTab = null)
        {
            await EnsureDefaultAdditionalWorkNormsAsync();

            var model = await BuildPageModelAsync(activeTab);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string? activeTab)
        {
            await EnsureDefaultAdditionalWorkNormsAsync();

            await SaveMainNormTimesFromFormAsync();
            await SaveAdditionalWorkNormsFromFormAsync();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Нормы времени сохранены";

            return RedirectToAction(nameof(Index), new
            {
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

                var hours = ReadDecimalFromForm(form[$"AdditionalWorkNorms[{index}].Hours"].ToString());

                dbItem.Hours = Math.Round(
                    Math.Max(0, hours),
                    2,
                    MidpointRounding.AwayFromZero);

                dbItem.IsDefault = false;
            }
        }

        private async Task<NormTimePageViewModel> BuildPageModelAsync(string? activeTab)
        {
            var items = await _context.NormTimes
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .Select(x => new NormTimeRowViewModel
                {
                    Id = x.Id,
                    WorkName = x.WorkName,
                    CategoryName = x.CategoryName,
                    CalculationBase = x.CalculationBase,
                    Hours = x.Hours
                })
                .ToListAsync();

            var additionalItems = await _context.AdditionalWorkNorms
                .AsNoTracking()
                .Where(x => AdditionalWorkCodes.Contains(x.Code))
                .ToListAsync();

            additionalItems = additionalItems
                .OrderBy(x => GetAdditionalWorkSortOrder(x.Code))
                .ToList();

            return new NormTimePageViewModel
            {
                Items = items,
                AdditionalWorkNorms = additionalItems
                    .Select(x => new AdditionalWorkNormRowViewModel
                    {
                        Id = x.Id,
                        WorkType = x.WorkType,
                        Code = x.Code,
                        Name = x.Name,
                        Hours = x.Hours
                    })
                    .ToList(),
                ActiveTab = NormalizeActiveTab(activeTab)
            };
        }

        private async Task EnsureDefaultAdditionalWorkNormsAsync()
        {
            var existingItems = await _context.AdditionalWorkNorms
                .ToListAsync();

            AddOrUpdateNorm(
                existingItems,
                AdditionalWorkType.PostgraduateSupervision,
                PostgraduateNormCode,
                "Аспиранты");

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
                    Hours = 0m,
                    IsDefault = false
                });

                return;
            }

            existingItem.WorkType = workType;
            existingItem.Name = name;
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
    }
}