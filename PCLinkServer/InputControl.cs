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
    public class InputControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private readonly InputSimulator sim = new InputSimulator();

        public void HandleCmd(string cmd, string[] param)
        {
            Console.WriteLine(cmd);
            switch (cmd)
            {
                case "MOVE":
                    if (param.Length == 2 &&
                        int.TryParse(param[0], out int dx) &&
                        int.TryParse(param[1], out int dy))
                    {
                        sim.Mouse.MoveMouseBy(dx, dy);
                    }
                    break;
                case "CLICK":
                    sim.Mouse.LeftButtonClick();
                    //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                    break;
                case "SCROLL":
                    if (int.TryParse(param[0], out int delta))
                    {
                        MouseScroll.SendVerticalScroll(delta);
                        //sim.Mouse.VerticalScroll(Math.Clamp(delta, -1, 1));
                    }
                    break;
                case "HSCROLL":
                    if (int.TryParse(param[0], out int hdelta))
                    {
                        MouseScroll.SendHorizontalScroll(hdelta);
                        //sim.Mouse.HorizontalScroll(Math.Clamp(delta, -1, 1));
                    }
                    break;
                case "RIGHTCLICK":
                    sim.Mouse.RightButtonClick();
                    //mouse_event(0x08 | 0x10, 0, 0, 0, UIntPtr.Zero); // RIGHTDOWN | RIGHTUP
                    break;
                case "ZOOM":
                    // можно эмулировать Ctrl + Scroll
                    if (int.TryParse(param[0], out int zdelta))
                    {
                        sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        sim.Mouse.VerticalScroll(zdelta);
                        sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    }
                    break;
                case "KEYPRESS":
                    if (Boolean.TryParse(param[1], out bool isShifted) && int.TryParse(param[0], out int virtualKeyCode))
                    {
                        if (isShifted)
                        {
                            var key = (VirtualKeyCode)virtualKeyCode;
                            Console.WriteLine(key);
                            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, key);
                        }
                        else
                        {
                            var key = (VirtualKeyCode)virtualKeyCode;
                            Console.WriteLine(virtualKeyCode.ToString());
                            Console.WriteLine(key.ToString());
                            sim.Keyboard.KeyPress(key); // нажимает и отпускает
                        }
                        // HandleReceivedChar(param[0].ToCharArray()[0]);
                    }
                    break;
                case "SPECIAL_KEY":
                    if(param[0].Equals("BACKSPACE"))
                        sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    else if(param[0].Equals("ENTER"))
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    else if(param[0].Equals("SPACE"))
                        sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                    else if(param[0].Equals("LANG_SWITCH"))
                        sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.MENU);
                    break;
                    
            }

        }
    }
}