using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.PortalMock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class DisciplinesController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly IAcademicPlanImportService _academicPlanImportService;

        public DisciplinesController(
            DepartmentLoadDbContext context,
            IAcademicPlanImportService academicPlanImportService)
        {
            _context = context;
            _academicPlanImportService = academicPlanImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Disciplines
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.DisciplineName.Contains(search) ||
                    x.DisciplineShortName.Contains(search));
            }

            var items = await query
                .OrderBy(x => x.DisciplineName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.LatestYear = await _academicPlanImportService.GetLatestYearAsync() ?? DateTime.Now.Year;

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(string year)
        {
            await _academicPlanImportService.ImportYearAsync(year);
            return RedirectToAction(nameof(Index));
        }
    }
}