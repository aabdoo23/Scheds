namespace Scheds.Models.Forum
{
    public class Faculty
    {
        public int FacultyId { get; set; }
        public string FacultyName { get; set; }

        public virtual List<Major> Majors { get; set; }
    }
}
