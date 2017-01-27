using System;

namespace TimeLogger.DTO
{
    internal class SessionValidationStatus
    {
        internal readonly bool isStartValid;
        internal readonly bool isEndValid;
        internal readonly DateTime earliestStart;
        internal readonly DateTime latestEnd;

        internal bool isValid { get { return (isStartValid && isEndValid); } }

        internal SessionValidationStatus(bool isStartValid, bool isEndValid, DateTime earliestStart, DateTime latestEnd)
        {
            this.isStartValid = isStartValid;
            this.isEndValid = isEndValid;
            this.earliestStart = earliestStart;
            this.latestEnd = latestEnd;
        }

        //--------------------------------------------------------------------------------
        //
        //      messages
        //
        //--------------------------------------------------------------------------------

        internal static string buildErrorMessage(SessionValidationStatus sessionStatus, Session session)
        {
            string result = "";
            int overlap;
            if (sessionStatus.isStartValid == false)
            {
                overlap = (int)(sessionStatus.earliestStart - session.start).TotalMinutes;
                result += "Start time invalid.\n" +
                            "Overlap with previous session of " + overlap.ToString() + " minutes";
            }
            if (sessionStatus.isEndValid == false)
            {
                if (result.Length > 0) result += "\n\n";
                overlap = (int)(session.start.AddMinutes(session.duration) - sessionStatus.latestEnd).TotalMinutes;
                result += "End time invalid.\n" +
                            "Overlap with next session of " + overlap.ToString() + " minutes";
            }
            return result;
        }

    }
}
