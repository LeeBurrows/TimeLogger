using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace TimeLogger
{
    public partial class App : Application
    {
        internal const string TIME_SEPARATOR = ":";
        internal const int MINUTES_BLOCK_SIZE = 5;
        internal const string LABEL_FOR_NO_TAG = "None";

        private void onApplicationStartup(object sender, StartupEventArgs e)
        {
            //ensure only a single instance is running
            Process proc = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcesses();
            int count = 0;
            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i].ProcessName == proc.ProcessName) count++;
            }
            if (count > 1)
            {
                MessageBox.Show("Application already running", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.Current.Shutdown();
            }
        }
    }
}
