namespace ElMaherQuranSchool.Models
{
    public class ParentStudent
    {
        public int ParentId { get; set; }
        public Parent Parent { get; set; } = null!;

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
    }
}
