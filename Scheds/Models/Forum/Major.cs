namespace Scheds.Models.Forum
{
    public class Major
    {
        public int MajorId { get; set; }
        public string MajorName { get; set; }

        public int FacultyId { get; set; }
        public virtual Faculty Faculty { get; set; }

        public virtual List<User> Users { get; set; }
    }
}
