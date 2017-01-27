using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TimeLogger.DTO;
using TimeLogger.Helpers;
using TimeLogger.Models;
using TimeLogger.Services;

namespace TimeLogger.Panels
{
    public partial class ReportPanel : UserControl
    {
        internal delegate void openSessionPanelRequestEvent(Session session);
        internal event openSessionPanelRequestEvent openSessionPanelRequest;

        private DataTable dataTable;

        public ReportPanel()
        {
            InitializeComponent();
            init();
        }

        //--------------------------------------------------------------------------------
        //
        //      initialisation
        //
        //--------------------------------------------------------------------------------

        private void init()
        {
            //hook up listeners
            DataService.instance.sessionTableChanged += sessionsTableChangedHandler;
            Model.instance.tagsChanged += tagsChangedHandler;
            //initialise gui
            setDisplay();
            setTagFilters();
        }

        //--------------------------------------------------------------------------------
        //
        //      gui
        //
        //--------------------------------------------------------------------------------

        private void setDisplay()
        {
            //calculate start/end times
            DateTime start = (DateTime)startDatePicker.SelectedDate;
            DateTime end = (DateTime)endDatePicker.SelectedDate;
            //check start is prior or equal to end
            if (DateTime.Compare(start, end) > 0)
            {
                MessageBox.Show("Invalid date range", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //get tag filter
            int tagFilterID = -1;
            if (tagFilter.SelectedItem != null)
                tagFilterID = (tagFilter.SelectedItem as Tag).id;
            //refresh displays
            setTableDisplay(start, end, tagFilterID);
            setSummaryDisplay(start, end, tagFilterID);
        }

        private void setTagFilters()
        {
            List<Tag> tags = new List<Tag>(Model.instance.tags);
            //add all and none options for filter
            tags.Insert(0, new Tag(-1, "* ALL TAGS *"));
            tags.Insert(1, new Tag(0, App.LABEL_FOR_NO_TAG));
            tagFilter.ItemsSource = tags;
            tagFilter.SelectedIndex = 0;
        }

        private void setTableDisplay(DateTime start, DateTime end, int tagFilterID)
        {
            //get and store data
            dataTable = DataService.instance.getSessions(start, end, tagFilterID);
            //pass data to display
            reportDataGrid.ItemsSource = dataTable.AsDataView();
        }

        private void setSummaryDisplay(DateTime start, DateTime end, int tagFilterID)
        {
            //get data
            int minsTotal = DataService.instance.getSessionsTotalDuration(start, end, tagFilterID);
            //display data
            reportMessageLabel.Content = "Start: " + start.ToString("dd-MM-yy") + ",   End: " + end.ToString("dd-MM-yy") + ",   Total: " + FormattingUtils.formatTime(minsTotal);
            //set export button visibility
            exportBtn.IsEnabled = (dataTable.Rows.Count > 0);
        }

        //--------------------------------------------------------------------------------
        //
        //      exporting
        //
        //--------------------------------------------------------------------------------

        private void exportData()
        {
            //build string data
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("ID,Start Time,Duration,Tag,Comments");
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                Session session = DatabaseResultMapper.rowToSession(dataTable.Rows[i]);
                builder.AppendLine(session.toExportString());
            }
            //save data to file
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar + "TimeLogger export data.csv";
            File.WriteAllText(path, builder.ToString());
            //show confirmation message
            MessageBox.Show("Data exported to:\n"+ path, "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        private void tagsChangedHandler()
        {
            //refresh data
            setTagFilters();
            setDisplay();
        }

        private void sessionsTableChangedHandler()
        {
            //refresh data
            setDisplay();
        }

        private void clickHandler(object sender, RoutedEventArgs e)
        {
            if (sender == submitBtn)
            {
                //refresh data
                setDisplay();
            }
            else if (sender == exportBtn)
            {
                //export data
                exportData();
            }
        }

        private void selectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            //if no selection, exit
            if (e.AddedItems.Count == 0) return;

            //find selected row
            DataRow row = (e.AddedItems[0] as DataRowView).Row;
            //get data from row
            Session session = DatabaseResultMapper.rowToSession(row);
            //dispatch event
            if (session != null) openSessionPanelRequest?.Invoke(session);
        }

    }
}
