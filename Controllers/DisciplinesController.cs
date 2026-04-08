using DepartmentLoadApp.Data;
using DepartmentLoadApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class DisciplinesController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public DisciplinesController(
            DepartmentLoadDbContext context)
        {
            _context = context;
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

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromAcademicPlan(string year)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}