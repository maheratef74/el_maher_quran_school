using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElMaherQuranSchool.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string ParentPhone { get; set; } = string.Empty;

        public int TotalMemorizedPages { get; set; } = 0;
        public int PointProgress { get; set; } = 0;

        public string? ProfileImageUrl { get; set; }

        public int? HalaqaId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Halaqa? Halaqa { get; set; }
        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
        public ICollection<SessionRecord> SessionRecords { get; set; } = new List<SessionRecord>();
    }
}
