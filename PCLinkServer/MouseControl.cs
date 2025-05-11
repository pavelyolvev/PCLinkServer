using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using PCLinkServer;
using WindowsInput;
using WindowsInput.Native;

namespace PCLink
{
    public class MouseControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private readonly InputSimulator sim = new InputSimulator();

        public void HandleCmd(string cmd)
        {
            Console.WriteLine(cmd);
            if (cmd.StartsWith("MOVE:"))
            {
                var coords = cmd.Substring(5).Split(',');
                if (coords.Length == 2 &&
                    int.TryParse(coords[0], out int dx) &&
                    int.TryParse(coords[1], out int dy))
                {
                    sim.Mouse.MoveMouseBy(dx, dy);
                }
            }
            else if (cmd == "CLICK")
            {
                sim.Mouse.LeftButtonClick();
                //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            }
            else if (cmd.StartsWith("SCROLL:"))
            {
                if (int.TryParse(cmd.Substring(7), out int delta))
                {
                    MouseScroll.SendVerticalScroll(delta);
                    //sim.Mouse.VerticalScroll(Math.Clamp(delta, -1, 1));
                }
            }
            else if (cmd.StartsWith("HSCROLL:"))
            {
                if (int.TryParse(cmd.Substring(8), out int delta))
                {
                    MouseScroll.SendHorizontalScroll(delta);
                    //sim.Mouse.HorizontalScroll(Math.Clamp(delta, -1, 1));
                }
            }
            else if (cmd == "RIGHTCLICK")
            {
                sim.Mouse.RightButtonClick();
                //mouse_event(0x08 | 0x10, 0, 0, 0, UIntPtr.Zero); // RIGHTDOWN | RIGHTUP
            }
            else if (cmd.StartsWith("ZOOM:"))
            {
                // можно эмулировать Ctrl + Scroll
                if (int.TryParse(cmd.Substring(5), out int delta))
                {
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Mouse.VerticalScroll(delta);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                }
            }

        }
    }
}