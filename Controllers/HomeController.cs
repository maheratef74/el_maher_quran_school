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
            .OrderBy(t => t.CreatedAt)
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

        var halaqatList = halaqas.Select(h => new {
            id = h.Id,
            name = h.Name,
            sheikh = h.Teacher != null ? h.Teacher.Name : "غير محدد",
            students = h.Students.Select(stu => new {
                id = stu.Id,
                name = stu.Name,
                awjoh = stu.TotalMemorizedPages,
                targetPages = h.TargetPages,
                pagesProgress = stu.PagesProgress,
                pointProgress = stu.PointProgress,
                isMyChild = stu.ParentPhone == phone,
                attendance = stu.SessionRecords.Any() ? (stu.SessionRecords.Count(r => r.IsPresent) * 100) / stu.SessionRecords.Count : 100,
                profileImageUrl = stu.ProfileImageUrl,
                // Taking last 8 sessions for the modal
                sessions = stu.SessionRecords.OrderByDescending(sr => sr.Session.SessionDate).Take(8).Select(sr => new {
                    date = sr.Session.SessionDate.ToString("yyyy/MM/dd"),
                    isPresent = sr.IsPresent,
                    score = sr.AttendanceScore,
                    note = sr.TeacherNote
                }).ToList()
            }).ToList()
        }).ToList();

        var names = parentStudents.Select(s => s.Name.Split(' ').FirstOrDefault()).ToList();
        var parentName = names.Count > 1 
            ? "ولي أمر الطلاب " + string.Join(" و ", names) 
            : "ولي أمر الطالب " + names.FirstOrDefault();

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
