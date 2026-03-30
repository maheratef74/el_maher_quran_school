using ElMaherQuranSchool.Models;
using Microsoft.EntityFrameworkCore;


namespace ElMaherQuranSchool.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Apply migrations 
            context.Database.Migrate();

            if (context.Parents.Any())
            {
                // Already seeded
                return;
            }

            // Seed Teacher
            var teacher = new Teacher { Name = "Sheikh Ahmad", PhoneNumber = "01000000001" };
            context.Teachers.Add(teacher);

            // Seed Halaqa
            var halaqa = new Halaqa { Name = "Al-Noor Group", Description = "Evening Quran memorization group", Schedule = "Mon/Wed/Fri 5PM-7PM", Teacher = teacher };
            context.Halaqas.Add(halaqa);

            // Seed Parent
            var parent = new Parent { Name = "Ali Mahmoud", PhoneNumber = "01000000002" };
            context.Parents.Add(parent);

            // Seed Student
            var student = new Student { Name = "Omar Ali", SerialNumber = "S-1001", TotalMemorizedPages = 150, Halaqa = halaqa };
            context.Students.Add(student);

            // Link Parent and Student
            context.ParentStudents.Add(new ParentStudent { Parent = parent, Student = student });

            // Seed Session
            var session = new Session { SessionDate = DateTime.UtcNow.AddDays(-2), Halaqa = halaqa };
            context.Sessions.Add(session);

            // Seed SessionRecord (Evaluation/Attendance)
            var record = new SessionRecord
            {
                Session = session,
                Student = student,
                IsPresent = true,
                AttendanceScore = 10,
                MemorizationScore = 95,
                TeacherNote = "Omar did an excellent job memorizing Surah Al-Kahf today."
            };
            context.SessionRecords.Add(record);

            context.SaveChanges();
        }
    }
}
