using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;

namespace VeraMonitorBacklight
{
    public partial class Form1 : Form
    {
        Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int WM_SYSCOMMAND = 0x0112;
        const int WM_POWERBROADCAST = 0x218;
        const int SC_MONITORPOWER = 0xf170;
        const int PBT_POWERSETTINGCHANGE = 0x8013;

        const string OnUrl = "http://10.10.1.10:3480/data_request?id=action&output_format=xml&DeviceNum=41&serviceId=urn:upnp-org:serviceId:SwitchPower1&action=SetTarget&newTargetValue=1";
        const string OffUrl = "http://10.10.1.10:3480/data_request?id=action&output_format=xml&DeviceNum=41&serviceId=urn:upnp-org:serviceId:SwitchPower1&action=SetTarget&newTargetValue=0";

        
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // tray icon only
            Visible = false;
            ShowInTaskbar = false;

            // see http://msdn.microsoft.com/en-us/library/windows/desktop/hh448380(v=vs.85).aspx
            // register to receive current monitor's display state changes
            RegisterPowerSettingNotification(this.Handle, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);
        }



        #region Win32 calls

        struct POWERBROADCAST_SETTING { public Guid PowerSetting; public UInt32 DataLength; public byte Data; }

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        #endregion



        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(POWERBROADCAST_SETTING));

                try
                {
                    WebClient wc = new WebClient();

                    switch (ps.Data)
                    {
                        // defined at http://msdn.microsoft.com/en-us/library/windows/desktop/hh448380(v=vs.85).aspx
                        case 0:
                            wc.DownloadData(OffUrl);
                            break;
                        case 1:
                            wc.DownloadData(OnUrl);
                            break;
                    }
                }
                catch
                {
                    // failed to communicate
                    // For now, just ignore. Might be nice to retry in the future.
                }

                
            }

            base.WndProc(ref m);
        }



        

        private void turnMonitorsOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendMessage(this.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        
    }

}
