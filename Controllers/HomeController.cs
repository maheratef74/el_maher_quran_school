using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElMaherQuranSchool.Models;

using Microsoft.EntityFrameworkCore;
using ElMaherQuranSchool.Data;

namespace ElMaherQuranSchool.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalStudents = await _context.Students.CountAsync();
        ViewBag.TotalHalaqas = await _context.Halaqas.CountAsync();
        ViewBag.TotalTeachers = await _context.Teachers.CountAsync();
        ViewBag.TotalMemorizers = await _context.Students.CountAsync(s => s.TotalMemorizedPages > 50);

        ViewBag.Teachers = await _context.Teachers
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetParentData(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Json(new { success = false });

        var parentStudents = await _context.Students
            .Where(s => s.ParentPhone == phone)
            .ToListAsync();

        if (!parentStudents.Any())
            return Json(new { success = false });
        var parentHalaqaIds = parentStudents.Where(s => s.HalaqaId.HasValue).Select(s => s.HalaqaId.GetValueOrDefault()).Distinct().ToList();

        var halaqas = await _context.Halaqas
            .Include(h => h.Teacher)
            .Include(h => h.Students)
                .ThenInclude(s => s.SessionRecords)
                    .ThenInclude(sr => sr.Session)
            .Where(h => parentHalaqaIds.Contains(h.Id))
            .ToListAsync();

        var halaqatList = halaqas.Select(h => {
            var myStudentsInHalaqa = h.Students.Where(s => s.ParentPhone == phone).ToList();
            var myStudentsNames = string.Join(" و ", myStudentsInHalaqa.Select(s => s.Name.Split(' ').FirstOrDefault()));

            return new {
                id = h.Id,
                name = h.Name,
                studentNames = myStudentsNames,
                sheikh = h.Teacher != null ? h.Teacher.Name : "غير محدد",
                students = h.Students.Select(stu => new {
                    id = stu.Id,
                    name = stu.Name,
                    gender = stu.Gender.ToString(),
                    awjoh = stu.TotalMemorizedPages,
                    targetPages = h.TargetPages,
                    points = stu.PointProgress,
                    isMyChild = stu.ParentPhone == phone,
                    attendance = stu.SessionRecords.Any() ? (stu.SessionRecords.Count(r => r.IsPresent) * 100) / stu.SessionRecords.Count : 100,
                    profileImageUrl = stu.ProfileImageUrl,
                    sessions = stu.SessionRecords.OrderByDescending(sr => sr.Session.SessionDate).Take(8).Select(sr => new {
                        date = sr.Session.SessionDate.ToString("yyyy/MM/dd"),
                        isPresent = sr.IsPresent,
                        score = sr.AttendanceScore,
                        note = sr.TeacherNote
                    }).ToList()
                }).ToList()
            };
        }).ToList();

        var names = parentStudents.Select(s => s.Name.Split(' ').FirstOrDefault()).ToList();
        var namesText = names.Count > 1 ? string.Join(" و ", names) : names.FirstOrDefault();
        
        string prefix = "الطلاب";
        if (parentStudents.Count == 1)
        {
            prefix = parentStudents[0].Gender == ElMaherQuranSchool.Models.Gender.Female ? "الطالبة" : "الطالب";
        }
        
        var parentName = $"مرحبا ولي أمر {prefix} ({namesText}) . جعل الله أبناءك من أهل القرآن";
        

        return Json(new {
            success = true,
            parentName = parentName,
            halaqat = halaqatList
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
