namespace Scheds.Infrastructure.Util
{
    static public class GlobalTimeUtil
    {
        public static int GetHour(string time)
        {
            int ind = time.IndexOf(':');
            return int.Parse(time[..ind]);
        }
        public static int GetMinute(string time)
        {
            int ind = time.IndexOf(':');
            return int.Parse(time.Substring(ind + 1, ind + 3));
        }
        public static string GetAmPm(string time)
        {
            int ind = time.IndexOf(':');
            return time[(ind + 4)..];
        }
        public static bool DayConflict(string cardStart, string cardEnd, string currTime)
        {

            int startHr, endHr, currHr, startMin, endMin, currMin;
            string startAMPM, endAMPM, currAMPM;
            startHr = GetHour(cardStart);
            endHr = GetHour(cardEnd);
            currHr = GetHour(currTime);
            startMin = GetMinute(cardStart);
            endMin = GetMinute(cardEnd);
            currMin = GetMinute(currTime);
            startAMPM = GetAmPm(cardStart);
            endAMPM = GetAmPm(cardEnd);
            currAMPM = GetAmPm(currTime);
            if (startAMPM == "PM" && startHr != 12) startHr += 12;
            if (endAMPM == "PM" && endHr != 12) endHr += 12;
            if (currAMPM == "PM" && currHr != 12) currHr += 12;
            if (currAMPM.CompareTo(startAMPM) < 0) return false;
            if (currAMPM.CompareTo(endAMPM) > 0) return false;
            if (currHr < startHr) return false;
            if (currHr == startHr && currMin < startMin) return false;
            if (currHr > endHr) return false;
            if (currHr == endHr && currMin > endMin) return false;

            return true;
        }
    }
}
