using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElMaherQuranSchool.Data;
using ElMaherQuranSchool.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ElMaherQuranSchool.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DashboardController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalHalaqas = await _context.Halaqas.CountAsync();
            ViewBag.TotalTeachers = await _context.Teachers.CountAsync();
            return View();
        }

        public async Task<IActionResult> Students()
        {
            var students = await _context.Students
                .Include(s => s.Halaqa)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            ViewBag.Halaqas = await _context.Halaqas.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(string Name, int? HalaqaId, int TotalMemorizedPages = 0, string ParentPhone = "", IFormFile? profileImage = null)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "اسم الطالب مطلوب");
                ViewBag.Halaqas = await _context.Halaqas.ToListAsync();
                return View();
            }

            string? imageUrl = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                string webRoot = _hostEnvironment.WebRootPath;
                // Double wwwroot check (common in some hosting environments)
                if (webRoot.EndsWith("wwwroot") && Directory.Exists(Path.Combine(webRoot, "..", "wwwroot")) && webRoot.Contains("\\wwwroot\\wwwroot"))
                {
                   // This is a sign of nesting. In most cases, we want to save into the parent's wwwroot if available.
                   // But let's just make sure we save to whatever WebRootPath is actually served.
                }

                string uploadsDir = Path.Combine(webRoot, "uploads", "students");
                
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                string filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                imageUrl = "/uploads/students/" + fileName;
            }

            // Generate Serial Number starting from 1 logic
            var maxId = await _context.Students.AnyAsync() ? await _context.Students.MaxAsync(s => s.Id) : 0;
            string serialNumber = (maxId + 1).ToString();

            var student = new Student
            {
                Name = Name,
                ParentPhone = ParentPhone ?? string.Empty,
                SerialNumber = serialNumber,
                TotalMemorizedPages = TotalMemorizedPages,
                HalaqaId = HalaqaId,
                ProfileImageUrl = imageUrl
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return RedirectToAction("Students");
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            ViewBag.Halaqas = await _context.Halaqas.ToListAsync();
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(int id, string Name, int? HalaqaId, int TotalMemorizedPages, string ParentPhone, IFormFile? profileImage)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "اسم الطالب مطلوب");
                ViewBag.Halaqas = await _context.Halaqas.ToListAsync();
                return View(student);
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(student.ProfileImageUrl))
                {
                    string oldPath = Path.Combine(_hostEnvironment.WebRootPath, student.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string webRoot = _hostEnvironment.WebRootPath;
                string uploadsDir = Path.Combine(webRoot, "uploads", "students");
                
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                string filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                student.ProfileImageUrl = "/uploads/students/" + fileName;
            }

            student.Name = Name;
            student.ParentPhone = ParentPhone ?? string.Empty;
            student.HalaqaId = HalaqaId;
            student.TotalMemorizedPages = TotalMemorizedPages;

            await _context.SaveChangesAsync();

            return RedirectToAction("Students");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.SessionRecords)
                .Include(s => s.ParentStudents)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            // Delete associated records first (due to Restrict constraint)
            if (student.SessionRecords.Any())
            {
                _context.SessionRecords.RemoveRange(student.SessionRecords);
            }

            if (student.ParentStudents.Any())
            {
                _context.ParentStudents.RemoveRange(student.ParentStudents);
            }

            // Delete profile image
            if (!string.IsNullOrEmpty(student.ProfileImageUrl))
            {
                string imagePath = Path.Combine(_hostEnvironment.WebRootPath, student.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الطالب بنجاح.";
            return RedirectToAction("Students");
        }

        [HttpGet]
        public async Task<IActionResult> StudentProgress(int id)
        {
            var student = await _context.Students
                .Include(s => s.Halaqa)
                .Include(s => s.SessionRecords)
                    .ThenInclude(sr => sr.Session)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            student.SessionRecords = student.SessionRecords.OrderByDescending(sr => sr.Session.SessionDate).ToList();

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> AddSessionRecord(int studentId, DateTime sessionDate, bool isPresent, int attendanceScore, string teacherNote)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) return NotFound();
            
            if (student.HalaqaId == null)
            {
                TempData["Error"] = "الطالب غير مسجل في حلقة، لا يمكن تسجيل حضوره.";
                return RedirectToAction("StudentProgress", new { id = studentId });
            }

            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.HalaqaId == student.HalaqaId && s.SessionDate.Date == sessionDate.Date);

            if (session == null)
            {
                session = new Session
                {
                    HalaqaId = student.HalaqaId.Value,
                    SessionDate = sessionDate.Date
                };
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();
            }

            var existingRecord = await _context.SessionRecords.FirstOrDefaultAsync(sr => sr.SessionId == session.Id && sr.StudentId == studentId);
            if (existingRecord != null)
            {
                TempData["Error"] = "تم تسجيل الحضور لهذا الطالب في هذا اليوم مسبقاً.";
                return RedirectToAction("StudentProgress", new { id = studentId });
            }

            var record = new SessionRecord
            {
                SessionId = session.Id,
                StudentId = studentId,
                IsPresent = isPresent,
                AttendanceScore = attendanceScore,
                MemorizationScore = 0,
                TeacherNote = teacherNote ?? string.Empty
            };

            _context.SessionRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تسجيل الحضور بنجاح.";
            return RedirectToAction("StudentProgress", new { id = studentId });
        }

        public async Task<IActionResult> Halaqas()
        {
            var halaqas = await _context.Halaqas
                .Include(h => h.Teacher)
                .Include(h => h.Students)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
            return View(halaqas);
        }

        [HttpGet]
        public async Task<IActionResult> AddHalaqa()
        {
            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddHalaqa(string Name, string Description, string Schedule, int? TeacherId, int TargetPages = 30)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "اسم الحلقة مطلوب");
                ViewBag.Teachers = await _context.Teachers.ToListAsync();
                return View();
            }

            var halaqa = new Halaqa
            {
                Name = Name,
                Description = Description ?? string.Empty,
                Schedule = Schedule ?? string.Empty,
                TeacherId = TeacherId,
                TargetPages = TargetPages
            };


            _context.Halaqas.Add(halaqa);
            await _context.SaveChangesAsync();

            return RedirectToAction("Halaqas");
        }

        public async Task<IActionResult> Teachers()
        {
            var teachers = await _context.Teachers
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
            return View(teachers);
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(string Name, string PhoneNumber, string Description, string Role)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "اسم المعلم مطلوب");
                return View();
            }

            var teacher = new Teacher
            {
                Name = Name,
                PhoneNumber = PhoneNumber ?? string.Empty,
                Description = Description ?? string.Empty,
                Role = Role
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction("Teachers");
        }

        [HttpGet]
        public async Task<IActionResult> EditTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> EditTeacher(int id, string Name, string PhoneNumber, string Description, string Role)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "اسم المعلم مطلوب");
                return View(teacher);
            }

            teacher.Name = Name;
            teacher.PhoneNumber = PhoneNumber ?? string.Empty;
            teacher.Description = Description ?? string.Empty;
            teacher.Role = Role;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Teachers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction("Teachers");
        }

        [HttpGet]
        public async Task<IActionResult> EditHalaqa(int id)
        {
            var halaqa = await _context.Halaqas.FindAsync(id);
            if (halaqa == null) return NotFound();
            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            return View(halaqa);
        }

        [HttpPost]
        public async Task<IActionResult> EditHalaqa(int id, string Name, string Description, string Schedule, int? TeacherId, int TargetPages = 30)
        {
            var halaqa = await _context.Halaqas.FindAsync(id);
            if (halaqa == null) return NotFound();

            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "اسم الحلقة مطلوب");
                ViewBag.Teachers = await _context.Teachers.ToListAsync();
                return View(halaqa);
            }

            halaqa.Name = Name;
            halaqa.Description = Description ?? string.Empty;
            halaqa.Schedule = Schedule ?? string.Empty;
            halaqa.TeacherId = TeacherId;
            halaqa.TargetPages = TargetPages;
            halaqa.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("Halaqas");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHalaqa(int id)
        {
            var halaqa = await _context.Halaqas
                .Include(h => h.Students)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (halaqa == null) return NotFound();

            // Explicitly free all students in this halaqa
            if (halaqa.Students != null)
            {
                foreach (var student in halaqa.Students)
                {
                    student.HalaqaId = null;
                }
            }

            _context.Halaqas.Remove(halaqa);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الحلقة بنجاح، وتحرير جميع الطلاب المسجلين بها.";
            return RedirectToAction("Halaqas");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePointProgress(int studentId, int newPoints)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.PointProgress = newPoints;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث النقاط بنجاح.";
            }
            else
            {
                TempData["Error"] = "لم يتم العثور على الطالب.";
            }
            return RedirectToAction("StudentProgress", new { id = studentId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePagesProgress(int studentId, int newPages)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.PagesProgress = newPages;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث الأوجه بنجاح.";
            }
            else
            {
                TempData["Error"] = "لم يتم العثور على الطالب.";
            }
            return RedirectToAction("StudentProgress", new { id = studentId });
        }
    }
}
