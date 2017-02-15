using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private Bitmap Get_screen()
        {
           // Size s = Screen.PrimaryScreen.Bounds.Size;
           // Bitmap bt = new Bitmap(s.Width, s.Height);
           // Graphics g = Graphics.FromImage(bt);
           //// g.CopyFromScreen(0, 0, 0, 0, s);
            return ScreenCapturer.Capture();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = Get_screen();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
