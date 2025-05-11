using System.Runtime.InteropServices;

namespace PCLinkServer;

public class MouseScroll
{
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    const uint INPUT_MOUSE = 0;
    const uint MOUSEEVENTF_WHEEL = 0x0800;
    const uint MOUSEEVENTF_HWHEEL = 0x01000;

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    public static void SendVerticalScroll(int delta)
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].mi = new MOUSEINPUT
        {
            dx = 0,
            dy = 0,
            mouseData = (uint)delta,
            dwFlags = MOUSEEVENTF_WHEEL,
            time = 0,
            dwExtraInfo = IntPtr.Zero
        };

        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public static void SendHorizontalScroll(int delta)
    {
        INPUT[] inputs = new INPUT[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].mi = new MOUSEINPUT
        {
            dx = 0,
            dy = 0,
            mouseData = (uint)delta,
            dwFlags = MOUSEEVENTF_HWHEEL,
            time = 0,
            dwExtraInfo = IntPtr.Zero
        };

        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

}