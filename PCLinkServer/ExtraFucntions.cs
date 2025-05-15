using System.Diagnostics;

namespace PCLinkServer;
using System.Runtime.InteropServices;

public class ExtraFucntions
{
    [DllImport("PowrProf.dll", SetLastError = true)]
    private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

    public static void SleepWindows()
    {
        // false = спящий режим, true = гибернация
        SetSuspendState(false, true, true);
    }
    public static void RestartWindows()
    {
        Process.Start(new ProcessStartInfo("shutdown", "/r /t 0")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }
    public static void ShutdownWindows()
    {
        Process.Start(new ProcessStartInfo("shutdown", "/s /t 0")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }
}