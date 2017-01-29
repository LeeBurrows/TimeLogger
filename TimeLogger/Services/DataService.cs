using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using TimeLogger.DTO;
using System.Collections.Generic;

namespace TimeLogger.Services
{
    internal sealed class DataService
    {
        //--------------------------------------------------------------------------------
        //
        //      wiring
        //
        //--------------------------------------------------------------------------------

        private static readonly DataService _instance = new DataService();
        internal static DataService instance { get { return _instance; } }
        static DataService() { }
        DataService()
        {
            init();
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        internal delegate void sessionsTableChangedEvent();
        internal event sessionsTableChangedEvent sessionTableChanged;

        private void sessionTableChangedTrigger()
        {
            sessionTableChanged?.Invoke();
        }

        internal delegate void tagsTableChangedEvent();
        internal event tagsTableChangedEvent tagsTableChanged;

        private void tagsTableChangedTrigger()
        {
            tagsTableChanged?.Invoke();
        }

        //--------------------------------------------------------------------------------
        //
        //      session CRUD
        //
        //--------------------------------------------------------------------------------

        internal void addSession(Session session)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "INSERT INTO Sessions (Start, Duration, TagID, Comments) VALUES (@start, @duration, @tagID, @comments)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@start", formatDateTimeForDatabase(session.start)));
            command.Parameters.Add(new SQLiteParameter("@duration", session.duration));
            command.Parameters.Add(new SQLiteParameter("@tagID", session.tagID));
            command.Parameters.Add(new SQLiteParameter("@comments", session.comments));
            command.ExecuteNonQuery();
            conn.Close();
            sessionTableChangedTrigger();
        }

        internal void updateSession(Session session)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "UPDATE Sessions SET Start = @start, Duration = @duration, TagID = @tagID, Comments = @comments WHERE ID = @id";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@id", session.id));
            command.Parameters.Add(new SQLiteParameter("@start", formatDateTimeForDatabase(session.start)));
            command.Parameters.Add(new SQLiteParameter("@duration", session.duration));
            command.Parameters.Add(new SQLiteParameter("@tagID", session.tagID));
            command.Parameters.Add(new SQLiteParameter("@comments", session.comments));
            command.ExecuteNonQuery();
            conn.Close();
            sessionTableChangedTrigger();
        }

        internal void deleteSession(int id)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "DELETE FROM Sessions WHERE ID = @id";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@id", id));
            command.ExecuteNonQuery();
            conn.Close();
            sessionTableChangedTrigger();
        }

        //--------------------------------------------------------------------------------
        //
        //      session validation
        //
        //--------------------------------------------------------------------------------

        internal SessionValidationStatus validateSession(Session session)
        {
            bool isStartValid = true;
            bool isEndValid = true;
            DateTime earliestStart = new DateTime();
            DateTime latestEnd = new DateTime();
            //get sessions either side of passed in session
            Session prevSession = getAdjacentSession(session, true);
            Session nextSession = getAdjacentSession(session, false);
            //validate start time
            if (prevSession != null)
            {
                earliestStart = prevSession.start.AddMinutes(prevSession.duration);
                isStartValid = DateTime.Compare(earliestStart, session.start) <= 0;
            }
            //validate end time
            if (nextSession != null)
            {
                latestEnd = nextSession.start;
                isEndValid = DateTime.Compare(session.start.AddMinutes(session.duration), latestEnd) <= 0;
            }
            //return
            return new SessionValidationStatus(isStartValid, isEndValid, earliestStart, latestEnd);
        }

        private Session getAdjacentSession(Session session, bool previousSession)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql;
            if (previousSession == true)
                sql = "SELECT * FROM Sessions WHERE Start <= @start AND ID != @id ORDER BY Start DESC LIMIT 1";
            else
                sql = "SELECT * FROM Sessions WHERE Start > @start AND ID != @id ORDER BY Start ASC LIMIT 1";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@start", formatDateTimeForDatabase(session.start)));
            command.Parameters.Add(new SQLiteParameter("@id", session.id));
            SQLiteDataAdapter adaptor = new SQLiteDataAdapter(command);
            DataSet ds = new DataSet();
            adaptor.Fill(ds);
            conn.Close();
            if (ds.Tables[0].Rows.Count == 1)
                return DatabaseResultMapper.rowToSession(ds.Tables[0].Rows[0]);
            else
                return null;
        }

        //--------------------------------------------------------------------------------
        //
        //      tag CRUD
        //
        //--------------------------------------------------------------------------------

        internal void addTag(Tag tag)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "INSERT INTO Tags (TagLabel) VALUES (@tagLabel)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@tagLabel", tag.label));
            command.ExecuteNonQuery();
            conn.Close();
            tagsTableChangedTrigger();
        }

        internal void updateTag(Tag tag)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "UPDATE Tags SET TagLabel = @tagLabel WHERE ID = @id";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@id", tag.id));
            command.Parameters.Add(new SQLiteParameter("@tagLabel", tag.label));
            command.ExecuteNonQuery();
            conn.Close();
            tagsTableChangedTrigger();
        }

        internal void deleteTag(int id)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "DELETE FROM Tags WHERE ID = @id";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@id", id));
            command.ExecuteNonQuery();
            //clear matching session tags
            sql = "UPDATE Sessions SET TagID = 0 WHERE TagID = @tagID";
            command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@tagID", id));
            command.ExecuteNonQuery();
            conn.Close();
            tagsTableChangedTrigger();
            sessionTableChangedTrigger();
        }

        //--------------------------------------------------------------------------------
        //
        //      summary info
        //
        //--------------------------------------------------------------------------------

        internal List<Tag> getTags()
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "SELECT * FROM Tags ORDER BY TagLabel ASC";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataAdapter adaptor = new SQLiteDataAdapter(command);
            DataSet ds = new DataSet();
            adaptor.Fill(ds);
            conn.Close();
            List<Tag> result = new List<Tag>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow row = ds.Tables[0].Rows[i];
                result.Add(DatabaseResultMapper.rowToTag(row));
            }
            return result;
        }

        internal int getSessionsTotalDuration(DateTime start, DateTime end, int filterTagID = -1)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "SELECT SUM(Duration) FROM Sessions WHERE Start >= @start AND Start <= @end";
            if (filterTagID != -1) sql += " AND TagID = @tagID";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@start", formatDateTimeForDatabase(start, true)));
            command.Parameters.Add(new SQLiteParameter("@end", formatDateTimeForDatabase(end, false)));
            if (filterTagID != -1) command.Parameters.Add(new SQLiteParameter("@tagID", filterTagID));
            int sumResult = 0;
            try
            {
                sumResult = Convert.ToInt32(command.ExecuteScalar());
            }
            catch { }
            conn.Close();
            return sumResult;
        }

        internal DataTable getSessions(DateTime start, DateTime end, int filterTagID)
        {
            SQLiteConnection conn = getOpenConnection();
            string sql = "SELECT * FROM Sessions WHERE Start >= @start AND Start <= @end";
            if (filterTagID != -1) sql += " AND TagID = @tagID";
            sql += " ORDER BY Start DESC";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.Parameters.Add(new SQLiteParameter("@start", formatDateTimeForDatabase(start, true)));
            command.Parameters.Add(new SQLiteParameter("@end", formatDateTimeForDatabase(end, false)));
            if (filterTagID != -1) command.Parameters.Add(new SQLiteParameter("@tagID", filterTagID));
            SQLiteDataAdapter adaptor = new SQLiteDataAdapter(command);
            DataSet ds = new DataSet();
            adaptor.Fill(ds);
            conn.Close();
            return ds.Tables[0];
        }

        //--------------------------------------------------------------------------------
        //
        //      storage
        //
        //--------------------------------------------------------------------------------

        private string databasePath;

        //--------------------------------------------------------------------------------
        //
        //      initialisation
        //
        //--------------------------------------------------------------------------------

        private const string DB_FOLDER_NAME = "TimeLogger";
        private const string DB_NAME = "TimeLoggerDatabase.db";

        private void init()
        {
            //check if DB file exists, and if not, create it
            databasePath = getDatabaseFolderPath() + Path.DirectorySeparatorChar + DB_NAME;
            if (File.Exists(databasePath) == false) createDatabase();
        }

        private void createDatabase()
        {
            //create folder
            if (Directory.Exists(getDatabaseFolderPath()) == false) Directory.CreateDirectory(getDatabaseFolderPath());
            //create database
            SQLiteConnection conn = getOpenConnection();
            conn.Close();
            //setup tables
            createTables();
        }

        private void createTables()
        {
            SQLiteCommand command;
            SQLiteConnection conn = getOpenConnection();
            command = new SQLiteCommand("CREATE TABLE Sessions (ID INTEGER PRIMARY KEY NOT NULL, Start TEXT NOT NULL, Duration INTEGER NOT NULL, TagID INTEGER NOT NULL, Comments TEXT NOT NULL)", conn);
            command.ExecuteNonQuery();
            command = new SQLiteCommand("CREATE TABLE Tags (ID INTEGER PRIMARY KEY NOT NULL, TagLabel TEXT NOT NULL)", conn);
            command.ExecuteNonQuery();
            conn.Close();
        }

        //--------------------------------------------------------------------------------
        //
        //      helpers
        //
        //--------------------------------------------------------------------------------

        private SQLiteConnection getOpenConnection()
        {
            string connString = "Data Source="+databasePath+";Version=3;";
            SQLiteConnection result = new SQLiteConnection(connString);
            result.Open();
            return result;
        }

        private string formatDateTimeForDatabase(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }

        private string formatDateTimeForDatabase(DateTime dateTime, bool startOfDay)
        {
            if (startOfDay)
                return dateTime.ToString("yyyy-MM-dd 00:00");
            else
                return dateTime.ToString("yyyy-MM-dd 23:59");
        }

        private string getDatabaseFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + DB_FOLDER_NAME;
        }

    }
}
