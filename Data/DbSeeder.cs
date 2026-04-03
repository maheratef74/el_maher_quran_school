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

            // Unified Teacher Data for seeding
            var teachersToSeed = new List<(string EmailName, string ArabicName, string Password)>
            {
                ("marwa.tharwat", "مروة ثروت", "marwa123"),
                ("yasmin.mohammad", "ياسمين محمد", "yasmin123"),
                ("fatma.ibrahim", "فاطمة إبراهيم", "fatma123"),
                ("rania.samir", "رانيا سمير", "rania123"),
                ("shahanda.taj", "شاهندة تاج الدين", "shahanda123"),
                ("asmaa.ahmad", "أسماء أحمد", "asmaa123"),
                ("aya.nasser", "آيه ناصر", "aya123"),
                ("sara.khaled", "سارة خالد", "sara123")
            };

            // 1. Seed AdminLogins
            foreach (var t in teachersToSeed)
            {
                string email = $"{t.EmailName}@el-maher.school";
                var existingUser = context.AdminLogins.FirstOrDefault(u => u.Email == email);
                if (existingUser == null)
                {
                    context.AdminLogins.Add(new AdminLogin { Email = email, Password = t.Password });
                }
                else
                {
                    // Update password if it's different (ensures our "unique easy passwords" are applied)
                    if (existingUser.Password != t.Password)
                    {
                        existingUser.Password = t.Password;
                    }
                }
            }
            
            // Add or update default developer login
            var adminUser = context.AdminLogins.FirstOrDefault(u => u.Email == "admin@el-maher.school");
            if (adminUser == null)
            {
                context.AdminLogins.Add(new AdminLogin { Email = "admin@el-maher.school", Password = "admin123" });
            }
            else
            {
                adminUser.Password = "admin123";
            }
            context.SaveChanges();

            // 2. Seed Teacher entities (so they appear in Halaqa assignment)
            foreach (var t in teachersToSeed)
            {
                if (!context.Teachers.Any(teacher => teacher.Name == t.ArabicName))
                {
                    context.Teachers.Add(new Teacher 
                    { 
                        Name = t.ArabicName, 
                        PhoneNumber = "01000000000",
                        Role = "Teacher"
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
