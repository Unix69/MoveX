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
                // var time = TimeSpan.FromSeconds(sec);

                var humanReadableStr = "";
                if (sec < 60)
                {
                    humanReadableStr = sec.ToString() + " sec.";
                }
                else if (sec > 60 && sec < 3600)
                {
                    humanReadableStr = (sec / 60).ToString() + " min, " + (sec % 60).ToString() + " sec.";
                } else if (sec > 3600 && sec < 7200)
                {
                    var hour = 1;
                    var remainingSec = sec - 3600;
                    humanReadableStr = "1 ora, " + (remainingSec / 60).ToString() + " min, " + (remainingSec % 60).ToString() + " sec.";
                } else if (sec > 7200)
                {
                    var hour = sec / 3600;
                    var remainingSec = sec - hour*3600;
                    humanReadableStr = hour + " ore, " + (remainingSec / 60).ToString() + " min, " + (remainingSec % 60).ToString() + " sec.";
                }

                // Return the string
                if (!string.IsNullOrEmpty(humanReadableStr)) return humanReadableStr;
                else return null;
            }
            catch (Exception e) { return null; }
        }
    }
}