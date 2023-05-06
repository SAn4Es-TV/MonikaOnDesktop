using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace MonikaOnDesktop.Models
{
    public class ScreenInformation
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ScreenRect : IEquatable<ScreenRect>
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public bool Equals(ScreenRect other)
            {
                throw new NotImplementedException();
            }
        }

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref ScreenRect pRect, int dwData);

        public class WpfScreen
        {
            public WpfScreen(ScreenRect prect)
            {
                metrics = prect;
            }

            private ScreenRect metrics;
        }

        static LinkedList<WpfScreen> allScreens = new LinkedList<WpfScreen>();

        public static LinkedList<WpfScreen> GetAllScreens()
        {
            ScreenInformation.GetMonitorCount();
            return allScreens;
        }

        public static int GetMonitorCount()
        {
            allScreens.Clear();
            int monCount = 0;
            MonitorEnumProc callback = (IntPtr _, IntPtr hdc, ref ScreenRect prect, int d) =>
            {
                Debug.WriteLine("Left {0}", prect.left);
                Debug.WriteLine("Right {0}", prect.right);
                Debug.WriteLine("Top {0}", prect.top);
                Debug.WriteLine("Bottom {0}", prect.bottom);
                allScreens.AddLast(new WpfScreen(prect));
                return ++monCount > 0;
            };

            if (EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0))
                Debug.WriteLine("You have {0} monitors", monCount);
            else
                Debug.WriteLine("An error occured while enumerating monitors");

            return monCount;
        }
    }
}
