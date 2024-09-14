namespace Scheds.Models
{
    static public class globals
    {
        public static int getHour(string time)
        {
            int ind = time.IndexOf(':');
            return int.Parse(time.Substring(0, ind));
        }
        public static int getMinute(string time)
        {
            int ind = time.IndexOf(':');
            return int.Parse(time.Substring(ind + 1, ind + 3));
        }
        public static string getAmPm(string time)
        {
            int ind = time.IndexOf(':');
            return time.Substring(ind + 4);
        }
        public static bool dayConflict(string cardStart, string cardEnd, string currTime)
        {

            int startHr, endHr, currHr, startMin, endMin, currMin;
            string startAMPM, endAMPM, currAMPM;
            startHr = getHour(cardStart);
            endHr = getHour(cardEnd);
            currHr = getHour(currTime);
            startMin = getMinute(cardStart);
            endMin = getMinute(cardEnd);
            currMin = getMinute(currTime);
            startAMPM = getAmPm(cardStart);
            endAMPM = getAmPm(cardEnd);
            currAMPM = getAmPm(currTime);
            if (startAMPM=="PM" && startHr != 12) startHr += 12;
            if (endAMPM == "PM" && endHr != 12) endHr += 12;
            if (currAMPM == "PM" && currHr != 12) currHr += 12;
            if (currAMPM.CompareTo(startAMPM)<0 ) return false;
            if (currAMPM.CompareTo(endAMPM) > 0) return false;
            if (currHr < startHr) return false;
            if (currHr == startHr && currMin < startMin) return false;
            if (currHr > endHr) return false;
            if (currHr == endHr && currMin > endMin) return false;

            return true;

        }
    }
}
