using System.ComponentModel.DataAnnotations;

namespace ElMaherQuranSchool.Models
{
    public class SessionRecord
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public int StudentId { get; set; }

        public bool IsPresent { get; set; } = true;

        [Range(5, 100)]
        public int AttendanceScore { get; set; } = 5;

        [Range(0, 100)]
        public int MemorizationScore { get; set; } = 0;

        [StringLength(500)]
        public string TeacherNote { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Session Session { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}
