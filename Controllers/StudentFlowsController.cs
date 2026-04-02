using DepartmentLoadApp.Data;
using DepartmentLoadApp.Models;
using DepartmentLoadApp.Models.Contingent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DepartmentLoadApp.Controllers
{
    public class StudentFlowsController : Controller
    {
        private readonly DepartmentLoadDbContext _context;

        public StudentFlowsController(DepartmentLoadDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.StudentFlows
                .AsNoTracking()
                .OrderBy(x => x.AcademicYear)
                .ThenBy(x => x.Course)
                .ThenBy(x => x.FlowName)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateFromContingent(int academicYear)
        {
            var contingents = await _context.ContingentRows
                .AsNoTracking()
                .OrderBy(x => x.DirectionCode)
                .ToListAsync();

            var oldRows = await _context.StudentFlows
                .Where(x => x.AcademicYear == academicYear.ToString()) 
                .ToListAsync();

            if (oldRows.Any())
            {
                _context.StudentFlows.RemoveRange(oldRows);
                await _context.SaveChangesAsync();
            }

            var rows = new List<StudentFlow>();

            rows.AddRange(BuildGeneralFlows(contingents, academicYear, true));
            rows.AddRange(BuildGeneralFlows(contingents, academicYear, false));

            foreach (var item in contingents)
            {
                rows.AddRange(BuildDirectionFlows(item, academicYear));
            }

            if (rows.Any())
            {
                await _context.StudentFlows.AddRangeAsync(rows);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new StudentFlow
            {
                AcademicYear = GetCurrentAcademicYear(),
                Course = 1, // По умолчанию 1-й курс
                EducationLevel = "Бакалавриат"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(StudentFlow model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.StudentFlows.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.StudentFlows.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StudentFlow model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dbItem = await _context.StudentFlows.FindAsync(model.Id);
            if (dbItem == null)
            {
                return NotFound();
            }

            dbItem.AcademicYear = model.AcademicYear;
            dbItem.FlowName = model.FlowName;
            dbItem.DirectionCode = model.DirectionCode;
            dbItem.Course = model.Course;
            dbItem.EducationLevel = model.EducationLevel;
            dbItem.GroupNames = model.GroupNames;
            dbItem.StudentsCount = model.StudentsCount;
            dbItem.GroupsCount = model.GroupsCount;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.StudentFlows.FindAsync(id);
            if (item != null)
            {
                _context.StudentFlows.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private static List<StudentFlow> BuildGeneralFlows(List<ContingentRow> contingents, int academicYear, bool isBachelor)
        {
            var rows = contingents
                .Where(x => isBachelor ? x.IsBachelor : x.IsMaster)
                .ToList();

            var educationLevel = isBachelor ? "Бакалавриат" : "Магистратура";
            var result = new List<StudentFlow>();

            for (int course = 1; course <= 4; course++)
            {
                int students = rows.Sum(x => GetStudents(x, course));
                int groups = rows.Sum(x => GetGroups(x, course));

                if (students == 0 && groups == 0)
                {
                    continue;
                }

                result.Add(new StudentFlow
                {
                    AcademicYear = academicYear.ToString(),
                    FlowName = $"Весь {course} курс {educationLevel.ToLower()}",
                    DirectionCode = "Все направления",
                    Course = course,
                    EducationLevel = educationLevel,
                    GroupNames = string.Empty,
                    StudentsCount = students,
                    GroupsCount = groups
                });
            }

            return result;
        }

        private static List<StudentFlow> BuildDirectionFlows(ContingentRow item, int academicYear)
        {
            var educationLevel = item.IsBachelor ? "Бакалавриат" : "Магистратура";
            var result = new List<StudentFlow>();

            for (int course = 1; course <= 4; course++)
            {
                int students = GetStudents(item, course);
                int groups = GetGroups(item, course);

                if (students == 0 && groups == 0)
                {
                    continue;
                }

                result.Add(new StudentFlow
                {
                    AcademicYear = academicYear.ToString(),
                    FlowName = $"{course} курс {educationLevel.ToLower()} {item.DirectionCode}",
                    DirectionCode = item.DirectionCode,
                    Course = course,
                    EducationLevel = educationLevel,
                    GroupNames = string.Empty,
                    StudentsCount = students,
                    GroupsCount = groups
                });
            }

            return result;
        }

        private static int GetStudents(ContingentRow row, int course)
        {
            return course switch
            {
                1 => row.Course1Count,
                2 => row.Course2Count,
                3 => row.Course3Count,
                4 => row.Course4Count,
                _ => 0
            };
        }

        private static int GetGroups(ContingentRow row, int course)
        {
            return course switch
            {
                1 => row.Course1Groups,
                2 => row.Course2Groups,
                3 => row.Course3Groups,
                4 => row.Course4Groups,
                _ => 0
            };
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