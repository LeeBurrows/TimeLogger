using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeLogger.Helpers;
using TimeLogger.Services;

namespace TimeLogger.Panels
{
    public partial class TitleBarPanel : UserControl
    {
        public TitleBarPanel()
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
            DataService.instance.sessionTableChanged += sessionTableChangedHandler;
            //gui
            setTimeTotalDisplays();
        }

        //--------------------------------------------------------------------------------
        //
        //      gui
        //
        //--------------------------------------------------------------------------------

        private void setTimeTotalDisplays()
        {
            //calculate start times for totals
            DateTime dayStart = DateTime.Now;
            int dayOfWeek = ((int)DateTime.Now.DayOfWeek + 6) % 7; // DayOfWeek returns 0 for sunday. want 0 for monday, so add 6 and take mod 7 to ensure result between 0 and 6
            DateTime weekStart = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).AddDays(-dayOfWeek);
            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            //get totals data
            int dayTotal = DataService.instance.getSessionsTotalDuration(dayStart, DateTime.Now);
            int weekTotal = DataService.instance.getSessionsTotalDuration(weekStart, DateTime.Now);
            int monthTotal = DataService.instance.getSessionsTotalDuration(monthStart, DateTime.Now);
            //update gui
            timeTotalDay.Content = FormattingUtils.formatTime(dayTotal);
            timeTotalWeek.Content = FormattingUtils.formatTime(weekTotal);
            timeTotalMonth.Content = FormattingUtils.formatTime(monthTotal);
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        private void sessionTableChangedHandler()
        {
            setTimeTotalDisplays();
        }

        private void clickHandler(object sender, RoutedEventArgs e)
        {
            //minimise app
            if (sender == minimiseBtn) App.Current.MainWindow.WindowState = WindowState.Minimized;
            //close app
            else if (sender == exitBtn) App.Current.MainWindow.Close();
        }

        private void mouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                //start window dragging
                if (sender == titleBorder)
                {
                    App.Current.MainWindow.DragMove();
                }
            }
        }

    }
}
