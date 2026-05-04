using System.Windows.Forms;

namespace HangulSwitcher;

internal static class Program
{
    private static readonly Mutex SingleInstance = new(true, AppInfo.MutexId);

    [STAThread]
    private static void Main()
    {
        if (!SingleInstance.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("이미 실행 중입니다.", AppInfo.AppName);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayContext());
        SingleInstance.ReleaseMutex();
    }
}
