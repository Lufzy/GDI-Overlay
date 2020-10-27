using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GDIOverlay
{
    class GDIOverlay
    {
        public static Form Overlay = new Form();
        private static bool IsInitialized = false;
        public static bool OverlayIsShowed = false;

        private static Process pProcess = default(Process);
        private static Structs.Rect WindowRect;
        private static IntPtr ProcessHandle = IntPtr.Zero;
        private static string ProcessName = string.Empty;
        private static string WindowName = string.Empty;
        private static int ProcessId = -1;

        public static void Initialize(string processName)
        {
            Process[] p = Process.GetProcessesByName(processName);
            if(p.Length != 0)
            {
                if(!IsInitialized)
                {
                    // set informations
                    pProcess = p.FirstOrDefault();
                    ProcessName = pProcess.ProcessName;
                    ProcessId = pProcess.Id;
                    WindowName = pProcess.MainWindowTitle;
                    ProcessHandle = Imports.FindWindow(null, WindowName);
                    Functions.SetDoubleBuffered(Overlay);

                    Overlay.BackColor = Color.FromArgb(2, 2, 2); 
                    Overlay.TransparencyKey = Color.FromArgb(2, 2, 2); 
                    Overlay.TopMost = true; 
                    Overlay.FormBorderStyle = FormBorderStyle.None; 
                    Overlay.ShowInTaskbar = false; 

                    Overlay.Show();
                    Overlay.Update();

                    // do unclickable
                    int initialStyle = Imports.GetWindowLong(Overlay.Handle, -20);
                    Imports.SetWindowLong(Overlay.Handle, -20, initialStyle | 0x80000 | 0x20);

                    Timer Update = Functions.CreateTimer(new EventHandler(Update_Overlay), 10);
                    Update.Start();

                    Timer NonShow = Functions.CreateTimer(new EventHandler(NonShow_Overlay), 20);
                    NonShow.Start();

                    Timer ProcessControl = Functions.CreateTimer(new EventHandler(ProcessControl_Overlay), 25);
                    ProcessControl.Start();

                    Console.Write(Environment.NewLine);
                    Console.WriteLine("[INFORMATION]");
                    Console.WriteLine("Window Name    => {0}", WindowName);
                    Console.WriteLine("Process Name   => {0}", ProcessName);
                    Console.WriteLine("Process Handle => 0x{0}", ProcessHandle.ToString("X"));
                    Console.WriteLine("Process Id     => {0}", ProcessId);
                    Console.WriteLine("[/INFORMATION]");
                    Console.Write(Environment.NewLine);

                    Imports.SetForegroundWindow(ProcessHandle);
                    OverlayIsShowed = true;
                    IsInitialized = true;
                }
            }
            else
            {
                OverlayIsShowed = false;
                IsInitialized = false;

                Console.Write(Environment.NewLine);
                Console.WriteLine("[ERROR] PROCESS NOT FOUND.");
                Console.Write(Environment.NewLine);
            }
        }

        private static void Update_Overlay(object sender, EventArgs e)
        {
            Functions.ResizeOverlay();
            Functions.Refresh();
        }

        private static void NonShow_Overlay(object sender, EventArgs e)
        {
            if(IsInitialized)
            {
                if(Functions.GetActiveWindowTitle() == WindowName)
                {
                    OverlayIsShowed = true;
                }
                else
                {
                    OverlayIsShowed = false;
                }
            }
        }

        private static void ProcessControl_Overlay(object sender, EventArgs e)
        {
            if(IsInitialized)
            {
                Process[] p = Process.GetProcessesByName(ProcessName);
                if(p.Length == 0)
                {
                    Console.Error.WriteLine("{0} not found.", ProcessName);
                    Environment.Exit(0);
                }
            }
        }

        public class Drawing
        {
            public static void DrawOutlineText(Graphics g, string Text, int x, int y, Font font, Brush textColor, Brush outlineColor)
            {
                g.DrawString(Text, font, outlineColor, new PointF(19, 19));
                g.DrawString(Text, font, outlineColor, new PointF(19, 18));
                g.DrawString(Text, font, outlineColor, new PointF(19, 21));
                g.DrawString(Text, font, outlineColor, new PointF(21, 21));
                g.DrawString(Text, font, outlineColor, new PointF(21, 22));
                g.DrawString(Text, font, outlineColor, new PointF(21, 20));
                g.DrawString(Text, font, outlineColor, new PointF(21, 19));
                g.DrawString(Text, font, textColor, new PointF(20, 20));
            }

            public static void DrawOutlineText(Graphics g, string text, Font font, float thickness, float x, float y, Brush colorText, Color outlineColor)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddString(text,
                    font.FontFamily, (int)font.Style,
                    g.DpiY * font.Size / 72, // convert to em size
                    new PointF(x, y), new StringFormat());
                g.DrawPath(new Pen(outlineColor, thickness), path);
                g.FillPath(colorText, path);
            }

            public static void DrawCircle(Graphics g, Pen pen, float centerX, float centerY, float radius)
            {
                g.DrawEllipse(pen, centerX - radius, centerY - radius,
                              radius + radius, radius + radius);
            }

            public static void FillCircle(Graphics g, Brush brush, float centerX, float centerY, float radius)
            {
                g.FillEllipse(brush, centerX - radius, centerY - radius,
                              radius + radius, radius + radius);
            }
        }

        public class Functions
        {
            public static void ResizeOverlay()
            {
                if(IsInitialized)
                {
                    Imports.GetWindowRect(ProcessHandle, out WindowRect);
                    Overlay.Size = new System.Drawing.Size(WindowRect.Right - WindowRect.Left/* - 15*/, WindowRect.Bottom - WindowRect.Top/* - 7*/);
                    Overlay.Top = WindowRect.Top;
                    Overlay.Left = WindowRect.Left/* + 7*/;
                }
            }

            public static void Refresh()
            {
                Overlay.Invalidate();
            }

            public static string GetActiveWindowTitle()
            {
                const int nChars = 256;
                StringBuilder Buff = new StringBuilder(nChars);
                IntPtr handle = Imports.GetForegroundWindow();

                if (Imports.GetWindowText(handle, Buff, nChars) > 0)
                {
                    return Buff.ToString();
                }
                return null;
            }

            public static void SetDoubleBuffered(System.Windows.Forms.Control c) // https://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form
            {
                //Taxes: Remote Desktop Connection and painting
                //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
                if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                    return;

                System.Reflection.PropertyInfo aProp =
                      typeof(System.Windows.Forms.Control).GetProperty(
                            "DoubleBuffered",
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                aProp.SetValue(c, true, null);
            }

            #region Other funcs

            public static Timer CreateTimer(EventHandler new_event, int Interval)
            {
                Timer timer = new Timer();
                timer.Tick += new_event;
                timer.Interval = Interval;
                return timer;
            }

            #endregion
        }

        private static class Structs
        {
            public struct Rect
            {
                public int Left, Top, Right, Bottom;
            }

        }

        private static class Imports
        {
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindow(string ipClassName, string ipWindowName);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hwnd, out Structs.Rect lpRect);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        }
    }
}
