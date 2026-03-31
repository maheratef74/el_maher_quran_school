using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElMaherQuranSchool.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Halaqa> Halaqas { get; set; } = new List<Halaqa>();
    }
}
