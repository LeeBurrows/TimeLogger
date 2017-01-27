using System;
using System.Data;
using TimeLogger.DTO;
using TimeLogger.Helpers;

namespace TimeLogger.Services
{
    internal static class DatabaseResultMapper
    {
        //map from sqlite table "Sessions" to Session DTO object
        internal static Session rowToSession(DataRow row)
        {
            if (row == null) return null;

            int id = Convert.ToInt32(row.ItemArray[0]);
            DateTime start = FormattingUtils.stringToDateTime(Convert.ToString(row.ItemArray[1]));
            int duration = Convert.ToInt32(row.ItemArray[2]);
            int tagID = Convert.ToInt32(row.ItemArray[3]);
            string comments = Convert.ToString(row.ItemArray[4]);
            return new Session(id, start, duration, tagID, comments);
        }

        //map from sqlite table "Tags" to Tag DTO object
        internal static Tag rowToTag(DataRow row)
        {
            if (row == null) return null;

            int id = Convert.ToInt32(row.ItemArray[0]);
            string name = Convert.ToString(row.ItemArray[1]);
            return new Tag(id, name);
        }
    }
}
