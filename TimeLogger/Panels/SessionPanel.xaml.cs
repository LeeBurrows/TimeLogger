using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TimeLogger.DTO;
using TimeLogger.Models;
using TimeLogger.Services;

namespace TimeLogger.Panels
{
    public partial class SessionPanel : UserControl
    {
        internal delegate void sessionPanelFinishedEvent();
        internal event sessionPanelFinishedEvent sessionPanelFinished;

        private Session currentSession;

        public SessionPanel()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------------------
        //
        //      initialisation
        //
        //--------------------------------------------------------------------------------

        internal void init(Session session)
        {
            //hook up listeners
            Model.instance.tagsChanged += tagsChangedHandler;
            //store data
            currentSession = session;
            //fill display
            sessionStartDate.SelectedDate = currentSession.start;
            sessionStartHours.SelectedIndex = currentSession.start.Hour;
            sessionStartMinutes.SelectedIndex = (int)Math.Floor(currentSession.start.Minute / (float)App.MINUTES_BLOCK_SIZE);
            sessionDurationHours.SelectedIndex = (int)Math.Floor(currentSession.duration / 60f);
            sessionDurationMinutes.SelectedIndex = (int)Math.Floor((currentSession.duration % 60) / (float)App.MINUTES_BLOCK_SIZE);
            sessionComments.Text = this.currentSession.comments;
            setTagsList();
        }

        //--------------------------------------------------------------------------------
        //
        //      crud
        //
        //--------------------------------------------------------------------------------

        private void deleteSession()
        {
            //delete item
            DataService.instance.deleteSession(currentSession.id);
        }

        private void saveSession()
        {
            //get data from form
            DateTime sessionDate = (DateTime)sessionStartDate.SelectedDate;
            int startHours = sessionStartHours.SelectedIndex;
            int startMinutes = sessionStartMinutes.SelectedIndex * App.MINUTES_BLOCK_SIZE;
            int durationHours = sessionDurationHours.SelectedIndex;
            int durationMinutes = sessionDurationMinutes.SelectedIndex * App.MINUTES_BLOCK_SIZE;
            currentSession.start = new DateTime(sessionDate.Year, sessionDate.Month, sessionDate.Day, startHours, startMinutes, 0);
            currentSession.duration = durationHours * 60 + durationMinutes;
            currentSession.tagID = (sessionTag.SelectedItem as Tag).id;
            currentSession.comments = sessionComments.Text;
            //validate data
            SessionValidationStatus sessionStatus = DataService.instance.validateSession(currentSession);
            if (sessionStatus.isValid == true)
            {
                //valid, so save data
                DataService.instance.updateSession(currentSession);
            }
            else
            {
                //invalid, so show error message
                string errorMsg = SessionValidationStatus.buildErrorMessage(sessionStatus, currentSession);
                MessageBox.Show(errorMsg, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void setTagsList()
        {
            List<Tag> tags = new List<Tag>(Model.instance.tags);
            //add none option
            tags.Insert(0, new Tag(0, App.LABEL_FOR_NO_TAG));
            sessionTag.ItemsSource = tags;
            sessionTag.SelectedItem = tags.Find(x => x.id == currentSession.tagID);
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        private void tagsChangedHandler()
        {
            setTagsList();
        }

        private void clickhandler(object sender, RoutedEventArgs e)
        {
            if (sender == cancelBtn)
            {
                //cancel editing and finish
                sessionPanelFinished?.Invoke();
            }
            else if (sender == deleteBtn)
            {
                //show confirmation message
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this session?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //if confirmed, delete item and finish
                if (result == MessageBoxResult.Yes)
                {
                    deleteSession();
                    sessionPanelFinished?.Invoke();
                }
            }
            else if (sender == submitBtn)
            {
                //save data and finish
                saveSession();
                sessionPanelFinished?.Invoke();
            }
        }

    }
}
