using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElMaherQuranSchool.Models
{
    public class Parent
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
