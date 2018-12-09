using System;

namespace Movex.View
{
    public static class HumanReadableTime
    {
        public static string MillisecToHumanReadable(long millisec)
        {
            try
            {
                // Sec to Human Readable String conversion
                var sec = (long) (millisec / 1000);
                var time = TimeSpan.FromSeconds(sec);

                var humanReadableStr = "";
                if (sec > 60)
                {
                     humanReadableStr = time.ToString(@"mm min, ss sec");
                } else if (sec > 3600)
                {   
                     humanReadableStr = time.ToString(@"hh ora, mm min, ss sec");
                } else if (sec > 7200)
                {   
                     humanReadableStr = time.ToString(@"hh ore, mm min, ss sec");
                }

                // Return the string
                if (!string.IsNullOrEmpty(humanReadableStr)) return humanReadableStr;
                else return null;
            }
            catch (Exception e) { return null; }
        }
    }
}