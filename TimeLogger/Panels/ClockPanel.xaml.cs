using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TimeLogger.DTO;
using TimeLogger.Helpers;
using TimeLogger.Models;
using TimeLogger.Services;

namespace TimeLogger.Panels
{
    public partial class ClockPanel : UserControl
    {
        private const int TICK_INTERVAL = 500;

        private DateTime sessionStartTime;
        private DispatcherTimer timer;

        public ClockPanel()
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
            Application.Current.Exit += appExitHandler;
            Model.instance.tagsChanged += tagsChangedHandler;
            //setup timer
            timer = new DispatcherTimer();
            timer.Tick += tickHandler;
            timer.Interval = new TimeSpan(0, 0, 0, 0, TICK_INTERVAL);
            //initialise gui
            setState(false);
            setClockDisplay(new TimeSpan());
            setTagsDisplay();
        }

        //--------------------------------------------------------------------------------
        //
        //      clock controls
        //
        //--------------------------------------------------------------------------------

        private void startClock()
        {
            //save start time, with minutes rounded down
            int startMinute = (int)Math.Floor(DateTime.Now.Minute / (float)App.MINUTES_BLOCK_SIZE) * App.MINUTES_BLOCK_SIZE;
            sessionStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, startMinute, 0);
            //start timer
            timer.Start();
            //update gui
            setClockDisplay(DateTime.Now - sessionStartTime);
            setState(true);
        }

        private void stopClock()
        {
            //stop timer
            timer.Stop();
            //save data
            saveSession();
            //update gui
            setClockDisplay(new TimeSpan());
            commentsInput.Text = "";
            setState(false);
        }

        //--------------------------------------------------------------------------------
        //
        //      crud
        //
        //--------------------------------------------------------------------------------

        private void saveSession()
        {
            //calculate duration, with minutes down
            int adjustedMinutes = (int)Math.Floor((DateTime.Now - sessionStartTime).TotalMinutes / (float)App.MINUTES_BLOCK_SIZE) * App.MINUTES_BLOCK_SIZE;
            //ensure minimum duration
            if (adjustedMinutes == 0) adjustedMinutes = App.MINUTES_BLOCK_SIZE;
            //build dto
            Session session = new Session(0, sessionStartTime, adjustedMinutes, (tagsCombo.SelectedItem as Tag).id, commentsInput.Text);
            //validate data
            SessionValidationStatus sessionStatus = DataService.instance.validateSession(session);
            if (sessionStatus.isValid == true)
            {
                //valid, so save data
                DataService.instance.addSession(session);
            }
            else
            {
                //invalid, so show error message
                string errorMsg = SessionValidationStatus.buildErrorMessage(sessionStatus, session);
                MessageBox.Show(errorMsg, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //--------------------------------------------------------------------------------
        //
        //      gui
        //
        //--------------------------------------------------------------------------------

        private void setState(bool isTicking)
        {
            //set clock button label
            StartStopBtn.Content = (isTicking == true) ? "Stop" : "Start";
            //set clock display font colour
            clockText.Foreground = (Brush)Application.Current.FindResource((isTicking == true) ? "ClockActive" : "ClockInactive");
        }

        private void setClockDisplay(TimeSpan timeSpan)
        {
            //update clock label content
            clockText.Content = FormattingUtils.formatTime(timeSpan, true);
        }



        private void setTagsDisplay()
        {
            //save current tag selection
            Tag selectedTag = tagsCombo.SelectedItem as Tag;
            int selectedTagID = (selectedTag != null) ? selectedTag.id : 0;
            //refresh data
            List<Tag> tags = new List<Tag>(Model.instance.tags);
            tags.Insert(0, new Tag(0, App.LABEL_FOR_NO_TAG));
            tagsCombo.ItemsSource = tags;
            //restore selection
            tagsCombo.SelectedItem = tags.Find(x => x.id == selectedTagID);
            if (tagsCombo.SelectedItem == null) tagsCombo.SelectedIndex = 0;
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        private void appExitHandler(object sender, ExitEventArgs e)
        {
            //if clock running, stop and save before exiting app
            if (timer.IsEnabled == true) stopClock();
        }

        private void tagsChangedHandler()
        {
            //refresh tags combo
            setTagsDisplay();
        }

        private void tickHandler(object source, EventArgs e)
        {
            //refresh clock label
            setClockDisplay(DateTime.Now - sessionStartTime);
        }

        private void clickHandler(object sender, RoutedEventArgs e)
        {
            //start/stop clock
            if (timer.IsEnabled == true)
                stopClock();
            else
                startClock();
        }

    }
}
