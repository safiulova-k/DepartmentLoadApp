using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class SemesterPeriodsController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public SemesterPeriodsController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.SemesterPeriods
                .AsNoTracking()
                .OrderBy(x => x.AcademicYear)
                .ThenBy(x => x.Season)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SemesterPeriod
            {
                AcademicYear = GetCurrentAcademicYear(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SemesterPeriod model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.SemesterPeriods.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.SemesterPeriods.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SemesterPeriod model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dbItem = await _context.SemesterPeriods.FindAsync(model.Id);
            if (dbItem == null)
            {
                return NotFound();
            }

            dbItem.AcademicYear = model.AcademicYear;
            dbItem.Season = model.Season;
            dbItem.StartDate = model.StartDate;
            dbItem.EndDate = model.EndDate;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SemesterPeriods.FindAsync(id);
            if (item != null)
            {
                _context.SemesterPeriods.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        private string GetCurrentAcademicYear()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;

            return month >= 9
                ? $"{year}-{year + 1}"
                : $"{year - 1}-{year}";
        }
    }
}