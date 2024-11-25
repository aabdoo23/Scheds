namespace Scheds.Domain.DTOs.SelfService
{
    public class SectionSearchParameters
    {
        public string keywords { get; set; }
        public string eventId { get; set; }
        public string eventType = "";
        public string eventSubType = "";
        public string campusId = "";
        public string classLevel = "";
        public string college = "";
        public string creditType = "";
        public string curriculum = "";
        public string department = "";
        public string endDate = "";
        public string endDateKey = "";
        public string endTime = "";
        public string generalEd = "";
        public string instructorId = "";
        public string meeting = "";
        public string nonTradProgram = "";
        public string period = "";
        public string population = "";
        public string program = "";
        public string registrationtype = "";
        public string session = "";
        public string startDate = "";
        public string startTime = "";
        public string status = "";
        public SectionSearchParameters(string keyWords)
        {
            eventId = keyWords;
            keywords = keyWords;
        }
    }
}
