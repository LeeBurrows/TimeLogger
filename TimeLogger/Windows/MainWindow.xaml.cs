using System.Windows;
using System.Windows.Input;
using TimeLogger.DTO;

namespace TimeLogger.Windows
{
    public partial class MainWindow : Window
    {
        private bool isAdminVisible;

        public MainWindow()
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
            reportPanel.openSessionPanelRequest += openSessionPanelRequestHandler;
            sessionPanel.sessionPanelFinished += sessionPanelFinishedHandler;
            //load and set window position
            loadWindowPosition();
            //gui
            isAdminVisible = false;
            showAdminPanels(isAdminVisible);
        }

        //--------------------------------------------------------------------------------
        //
        //      gui
        //
        //--------------------------------------------------------------------------------

        private void showAdminPanels(bool flag)
        {
            reportPanel.Visibility = boolToVisibility(flag);
            tagsPanel.Visibility = boolToVisibility(flag);
            //clear report dg selection
            reportPanel.reportDataGrid.SelectedItem = null;
            //if closing, close session panel too
            if (flag == false) showSessionPanel(false);
            //update expander button
            setExpanderButton(flag);
        }

        private void showSessionPanel(bool flag)
        {
            sessionPanel.Visibility = boolToVisibility(flag);
            //set report panel disabled if session panel open
            reportPanel.IsEnabled = !flag;
            //clear report dg selection
            reportPanel.reportDataGrid.SelectedItem = null;
        }

        private void setExpanderButton(bool flag)
        {
            expanderPathOpen.Visibility = boolToVisibility(!flag);
            expanderPathClose.Visibility = boolToVisibility(flag);
        }

        private Visibility boolToVisibility(bool flag)
        {
            return (flag == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        //--------------------------------------------------------------------------------
        //
        //      window position
        //
        //--------------------------------------------------------------------------------

        private void loadWindowPosition()
        {
            //set window position from app settings
            int left = Properties.Settings.Default.MainWindowLeft;
            int top = Properties.Settings.Default.MainWindowTop;
            if (left == -1 || top == -1) return;
            this.Left = left;
            this.Top = top;
        }

        private void saveWindowPosition()
        {
            //save window position to app settings
            Properties.Settings.Default.MainWindowLeft = (int)this.Left;
            Properties.Settings.Default.MainWindowTop = (int)this.Top;
            Properties.Settings.Default.Save();
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        private void appExitHandler(object sender, ExitEventArgs e)
        {
            //save current window position before exiting app
            saveWindowPosition();
        }

        private void openSessionPanelRequestHandler(Session session)
        {
            //set edit panel data
            sessionPanel.init(session);
            //open edit panel
            showSessionPanel(true);
        }

        private void sessionPanelFinishedHandler()
        {
            //close edit panel
            showSessionPanel(false);
        }

        private void mouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                //show/hide reports panel
                if (sender == expanderBorder)
                {
                    showAdminPanels(reportPanel.Visibility == Visibility.Collapsed);
                }
            }
        }


    }
}
