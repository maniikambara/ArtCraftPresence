using System.Runtime.InteropServices;
using System.Text;

namespace ArtCraftPresence;

/// <summary>Win32 helpers to read window titles and the foreground window.</summary>
internal static class NativeWindow
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    private const uint GW_OWNER = 4;

    /// <summary>Process id that owns the foreground window, or 0.</summary>
    public static int ForegroundProcessId()
    {
        var hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero) return 0;
        GetWindowThreadProcessId(hWnd, out var pid);
        return (int)pid;
    }

    /// <summary>Titles of all visible, non-empty top-level windows of a process (main window first is not guaranteed).</summary>
    public static List<string> GetWindowTitles(int processId)
    {
        var titles = new List<string>();
        var buffer = new StringBuilder(1024);

        EnumWindows((hWnd, _) =>
        {
            GetWindowThreadProcessId(hWnd, out var pid);
            if (pid != (uint)processId || !IsWindowVisible(hWnd) || GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
                return true;

            buffer.Clear();
            GetWindowText(hWnd, buffer, buffer.Capacity);
            var text = buffer.ToString();
            if (!string.IsNullOrWhiteSpace(text)) titles.Add(text);
            return true;
        }, IntPtr.Zero);

        return titles;
    }
}
