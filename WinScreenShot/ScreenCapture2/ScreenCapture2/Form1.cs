using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenCapture2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;
            MyProcess p = lstProcesses.SelectedItem as MyProcess;
            if (p == null) return;
            Process pro = p.Process;
            System.Drawing.Bitmap bmp = ScreenCapturere.GetScreenshot(pro.MainWindowHandle);
            //bmp.Save(@"d:\printscreen.jpg");
            DateTime end1 = DateTime.Now;
            //draw.Source = ScreenCapturere.Convert(bmp);
            //DateTime end2 = DateTime.Now;
            picDraw.Image = bmp;
            //lblTime.Content = string.Format("{0:0.00}, {1:0.00}", end1.Subtract(start).TotalMilliseconds, end2.Subtract(start).TotalMilliseconds);
            System.GC.Collect();
        }

        private void btnGetProcesses_Click(object sender, EventArgs e)
        {
            lstProcesses.Items.Clear();
            foreach (Process pro in Process.GetProcesses())
            {
                if (pro.MainWindowTitle != "")
                {
                    lstProcesses.Items.Add(new MyProcess(pro.MainWindowTitle, pro));
                }
            }
            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            
            if (!timer1.Enabled)
            {
                timer1.Start();
                btnStart.Text = "&Stop";
            }
            else
            {
                timer1.Stop();
                btnStart.Text = "&Start";
            }

        }
    }
}
