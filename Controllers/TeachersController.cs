using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.AcademicPlanImport;
using DepartmentLoadApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class TeachersController : Controller
    {
        private readonly DepartmentLoadDbContext _context;
        private readonly ILecturerImportService _lecturerImportService;

        public TeachersController(
            DepartmentLoadDbContext context,
            ILecturerImportService lecturerImportService)
        {
            _context = context;
            _lecturerImportService = lecturerImportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await _context.Teachers.AnyAsync())
            {
                await _lecturerImportService.ImportAsync();
            }

            var items = await _context.Teachers
                .AsNoTracking()
                .OrderBy(x => x.FullName)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromMock()
        {
            await _lecturerImportService.ImportAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Teacher());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Teacher model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Teachers.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Teachers.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Teacher model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dbItem = await _context.Teachers.FindAsync(model.Id);
            if (dbItem == null)
            {
                return NotFound();
            }

            dbItem.ExternalLecturerId = model.ExternalLecturerId;
            dbItem.FullName = model.FullName;
            dbItem.Position = model.Position;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Teachers.FindAsync(id);
            if (item != null)
            {
                _context.Teachers.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}