namespace Scheds.Models.SelfService
{
    public class SearchRequest
    {
        public SectionSearchParameters sectionSearchParameters;
        public int startIndex { get; set;}
        public int length { get; set;}
        public SearchRequest(string courseCode)
        {
            sectionSearchParameters = new SectionSearchParameters(courseCode);
            startIndex = 0;
            length = 100;
        }
    }
}
