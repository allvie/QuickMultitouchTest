
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Permissions;

namespace PeterPetkov.QuickMultitouchTest
{
    
    public class WMTouchForm : Form
    {
        [SecurityPermission(SecurityAction.Demand)]
        public WMTouchForm()
        {
            
            Load += new System.EventHandler(this.OnLoadHandler);
            touchInputSize = Marshal.SizeOf(new TOUCHINPUT());
        }
        
        protected event EventHandler<WMTouchEventArgs> Touchdown;   
        protected event EventHandler<WMTouchEventArgs> Touchup;     
        protected event EventHandler<WMTouchEventArgs> TouchMove;   
        protected class WMTouchEventArgs : System.EventArgs
        {
            
            private int x;                  
            private int y;                  
            private int id;                 
            private int mask;               
            private int flags;              
            private int time;               
            private int contactX;           
            private int contactY;           
            public int LocationX
            {
                get { return x; }
                set { x = value; }
            }
            public int LocationY
            {
                get { return y; }
                set { y = value; }
            }
            public int Id
            {
                get { return id; }
                set { id = value; }
            }
            public int Flags
            {
                get { return flags; }
                set { flags = value; }
            }
            public int Mask
            {
                get { return mask; }
                set { mask = value; }
            }
            public int Time
            {
                get { return time; }
                set { time = value; }
            }
            public int ContactX
            {
                get { return contactX; }
                set { contactX = value; }
            }
            public int ContactY
            {
                get { return contactY; }
                set { contactY = value; }
            }
            public bool IsPrimaryContact
            {
                get { return (flags & TOUCHEVENTF_PRIMARY) != 0; }
            }
            public WMTouchEventArgs()
            {
            }
        }
        
        private const int WM_TOUCH = 0x0240;
        private const int TOUCHEVENTF_MOVE = 0x0001;
        private const int TOUCHEVENTF_DOWN = 0x0002;
        private const int TOUCHEVENTF_UP = 0x0004;
        private const int TOUCHEVENTF_INRANGE = 0x0008;
        private const int TOUCHEVENTF_PRIMARY = 0x0010;
        private const int TOUCHEVENTF_NOCOALESCE = 0x0020;
        private const int TOUCHEVENTF_PEN = 0x0040;
        private const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 0x0001; 
        private const int TOUCHINPUTMASKF_EXTRAINFO = 0x0002; 
        private const int TOUCHINPUTMASKF_CONTACTAREA = 0x0004; 
        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public System.IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public System.IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTS
        {
            public short x;
            public short y;
        }
        
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterTouchWindow(System.IntPtr hWnd, ulong ulFlags);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTouchInputInfo(System.IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern void CloseTouchInputHandle(System.IntPtr lParam);
        private int touchInputSize;        
        private void OnLoadHandler(Object sender, EventArgs e)
        {
            try
            {
                if (!RegisterTouchWindow(this.Handle, 0))
                {
                    Debug.Print("ERROR: Could not register window for multi-touch");
                }
            }
            catch (Exception exception)
            {
                Debug.Print("ERROR: RegisterTouchWindow API not available");
                Debug.Print(exception.ToString());
                MessageBox.Show("RegisterTouchWindow API not available", "MTScratchpadWMTouch ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
            }
        }
        
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            
            bool handled;
            switch (m.Msg)
            {
                case WM_TOUCH:
                    handled = DecodeTouch(ref m);
                    break;
                default:
                    handled = false;
                    break;
            }
            base.WndProc(ref m);

            if (handled)
            {
                
                m.Result = new System.IntPtr(1);
            }
        }
        private static int LoWord(int number)
        {
            return (number & 0xffff);
        }
        
        private bool DecodeTouch(ref Message m)
        {
            int inputCount = LoWord(m.WParam.ToInt32()); 

            TOUCHINPUT[] inputs; 
            inputs = new TOUCHINPUT[inputCount]; 
            
            if (!GetTouchInputInfo(m.LParam, inputCount, inputs, touchInputSize))
            {
                
                return false;
            }
            
            bool handled = false; 
            for (int i = 0; i < inputCount; i++)
            {
                TOUCHINPUT ti = inputs[i];
                EventHandler<WMTouchEventArgs> handler = null;     
                if ((ti.dwFlags & TOUCHEVENTF_DOWN) != 0)
                {
                    handler = Touchdown;
                }
                else if ((ti.dwFlags & TOUCHEVENTF_UP) != 0)
                {
                    handler = Touchup;
                }
                else if ((ti.dwFlags & TOUCHEVENTF_MOVE) != 0)
                {
                    handler = TouchMove;
                }
                if (handler != null)
                {
                    
                    WMTouchEventArgs te = new WMTouchEventArgs(); 
                    
                    te.ContactY = ti.cyContact/100;
                    te.ContactX = ti.cxContact/100;
                    te.Id = ti.dwID;
                    {
                        Point pt = PointToClient(new Point(ti.x/100, ti.y/100));
                        te.LocationX = pt.X;
                        te.LocationY = pt.Y;
                    }
                    te.Time = ti.dwTime;
                    te.Mask = ti.dwMask;
                    te.Flags = ti.dwFlags;
                    handler(this, te);
                    handled = true;
                }
            }

            CloseTouchInputHandle(m.LParam);

            return handled;
        }
    }
}
