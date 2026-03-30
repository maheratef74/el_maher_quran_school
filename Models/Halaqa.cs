using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElMaherQuranSchool.Models
{
    public class Halaqa
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string Schedule { get; set; } = string.Empty; // e.g., "Mon-Wed 5PM-7PM"

        public int? TeacherId { get; set; }

        public int TargetPages { get; set; } = 30; // Default goal: 30 pages/awjoh

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Teacher? Teacher { get; set; }
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
