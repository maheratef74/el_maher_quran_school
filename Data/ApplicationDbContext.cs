using ElMaherQuranSchool.Models;
using Microsoft.EntityFrameworkCore;

namespace ElMaherQuranSchool.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }
        public DbSet<Halaqa> Halaqas { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionRecord> SessionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Naming conventions / Constraints (Fluent API)

            modelBuilder.Entity<ParentStudent>()
                .HasKey(ps => new { ps.ParentId, ps.StudentId });

            modelBuilder.Entity<ParentStudent>()
                .HasOne(ps => ps.Parent)
                .WithMany(p => p.ParentStudents)
                .HasForeignKey(ps => ps.ParentId);

            modelBuilder.Entity<ParentStudent>()
                .HasOne(ps => ps.Student)
                .WithMany(s => s.ParentStudents)
                .HasForeignKey(ps => ps.StudentId);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Halaqa)
                .WithMany(h => h.Students)
                .HasForeignKey(s => s.HalaqaId)
                .OnDelete(DeleteBehavior.SetNull); // Safe soft delete of relation

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Halaqa)
                .WithMany(h => h.Sessions)
                .HasForeignKey(s => s.HalaqaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionRecord>()
                .HasOne(sr => sr.Session)
                .WithMany(s => s.SessionRecords)
                .HasForeignKey(sr => sr.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionRecord>()
                .HasOne(sr => sr.Student)
                .WithMany(s => s.SessionRecords)
                .HasForeignKey(sr => sr.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete of student from session record

            // Indexes
            modelBuilder.Entity<Parent>()
                .HasIndex(p => p.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.PhoneNumber);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.SerialNumber)
                .IsUnique();
                
            modelBuilder.Entity<SessionRecord>()
                .HasIndex(sr => new { sr.SessionId, sr.StudentId })
                .IsUnique(); // A student can only have one record per session
        }
    }
}
