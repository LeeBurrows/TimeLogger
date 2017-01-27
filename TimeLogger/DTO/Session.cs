using System;
using TimeLogger.Helpers;
using TimeLogger.Models;

namespace TimeLogger.DTO
{
    //maps to sqlite table "Sessions"
    internal class Session
    {
        internal readonly int id;
        internal DateTime start;
        internal int duration;
        internal int tagID;
        internal string comments;

        internal Session(int id, DateTime start, int duration, int tagID, string comments)
        {
            this.id = id;
            this.start = start;
            this.duration = duration;
            this.tagID = tagID;
            this.comments = comments;
        }

        public string toExportString()
        {
            string tagLabel = App.LABEL_FOR_NO_TAG;
            if (tagID != 0) tagLabel = Model.instance.getTagByID(tagID).label;
            return id + "," + start.ToString() + "," + duration + "," + tagLabel + "," + FormattingUtils.removeNewlines(comments);
        }

        override public string ToString()
        {
            return id + "," + start.ToString() + "," + duration + "," + tagID + "," + FormattingUtils.removeNewlines(comments);
        }
    }
}
