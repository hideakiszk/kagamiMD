using System.Runtime.InteropServices;

namespace KagamiMD.Interop;

public static class EditorInterop
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int[]? lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    private const int EM_SETTABSTOPS = 0x00CB;
    private const int EM_LINESCROLL = 0x00B6;
    private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
    private const int WM_SETREDRAW = 0x000B;

    public static void SetTabWidth(IntPtr handle, int tabWidth)
    {
        // 1 tab stops unit is 1/4 of the average character width.
        // So for 4 characters, it's 4 * 4 = 16.
        int[] tabStops = { tabWidth * 4 };
        SendMessage(handle, EM_SETTABSTOPS, 1, tabStops);
    }

    public static int GetFirstVisibleLine(IntPtr handle)
    {
        return (int)SendMessage(handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero);
    }

    public static void ScrollLines(IntPtr handle, int lines)
    {
        SendMessage(handle, EM_LINESCROLL, 0, (IntPtr)lines);
    }

    public static void SuspendDrawing(IntPtr handle)
    {
        SendMessage(handle, WM_SETREDRAW, 0, IntPtr.Zero);
    }

    public static void ResumeDrawing(IntPtr handle)
    {
        SendMessage(handle, WM_SETREDRAW, 1, IntPtr.Zero);
    }
}
