using System;
using System.Data;
using System.Windows;
using TimeLogger.DTO;

namespace TimeLogger.Helpers
{
    internal static class FormattingUtils
    {
        //--------------------------------------------------------------------------------
        //
        //      formatting
        //
        //--------------------------------------------------------------------------------

        internal static string formatTime(TimeSpan timeSpan, bool includeSeconds = false)
        {
            int hours = (int)timeSpan.TotalHours;
            int mins = (int)timeSpan.Minutes;
            string result = hours.ToString("00") + App.TIME_SEPARATOR + mins.ToString("00");
            if (includeSeconds)
            {
                result += App.TIME_SEPARATOR + timeSpan.Seconds.ToString("00");
            }
            return result;
        }

        internal static string formatTime(int numMinutes, bool includeSeconds = false)
        {
            return formatTime(new TimeSpan(0, numMinutes, 0), includeSeconds);
        }

        internal static DateTime stringToDateTime(string value)
        {
            int year = int.Parse(value.Substring(0, 4));
            int month = int.Parse(value.Substring(5, 2));
            int day = int.Parse(value.Substring(8, 2));
            int hour = int.Parse(value.Substring(11, 2));
            int minute = int.Parse(value.Substring(14, 2));
            return new DateTime(year, month, day, hour, minute, 0);
        }

        //--------------------------------------------------------------------------------
        //
        //      datagrid formatting
        //
        //--------------------------------------------------------------------------------

        internal static string formatReportStartTime(string value)
        {
            string year = value.Substring(0, 4);
            string month = value.Substring(5, 2);
            string day = value.Substring(8, 2);
            string hour = value.Substring(11, 2);
            string minute = value.Substring(14, 2);
            return day + "/" + month + "/" + year + " " + hour + ":" + minute;
        }

        internal static string formatReportDuration(int value)
        {
            int hours = value / 60;
            int minutes = value - (hours * 60);
            return hours.ToString("00") + App.TIME_SEPARATOR + minutes.ToString("00");
        }

        internal static string removeNewlines(string value)
        {
            return value.Replace(System.Environment.NewLine, "   ");
        }

    }
}
