using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElMaherQuranSchool.Models
{
    public class Session
    {
        public int Id { get; set; }

        public int HalaqaId { get; set; }

        public DateTime SessionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Halaqa Halaqa { get; set; } = null!;
        public ICollection<SessionRecord> SessionRecords { get; set; } = new List<SessionRecord>();
    }
}
