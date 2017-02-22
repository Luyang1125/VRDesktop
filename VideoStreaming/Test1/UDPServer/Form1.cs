#define MULTIPOINTS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common;


namespace UDPServer
{
    public partial class Form1 : Form
    {
        private static float SCALE = .5f;
        private static readonly int WIDTH = (int)(Constants.PHONE_RESOLUTION_WIDTH * SCALE), HEIGHT = (int)(Constants.PHONE_RESOLUTION_HEIGHT * SCALE);
        private static readonly Size DEFAULT_SIZE = new Size(WIDTH, HEIGHT);
        private static readonly Rectangle DEFAULT_RECTANGLE = new Rectangle(0, 0, WIDTH, HEIGHT);
        private static readonly PointF CENTER = new PointF(WIDTH / 2f, HEIGHT / 2f);
        private static readonly Pen LINE_PEN = new Pen(Brushes.Blue, 3);
        private static readonly int PKT_SIZE = 64512;
        private static readonly Color BG_COLOR = Color.FromArgb(255, 213, 255);


        private static readonly int SLEEP = 1000 / 50;
        public static int COPIES = 16;

        private static readonly float[][] colorMatrixElements = {
                new float[] {1.0f,  0,  0,  0, 0},         // red scaling factor of 1.1
                new float[] {0,  1,  0,  0, 0},         // green scaling factor of 1
                new float[] {0,  0,  1,  0, 0},         // blue scaling factor of 1
                new float[] {0,  0,  0,  .99f, 0},      // alpha scaling factor of 0.93
                new float[] {.05f, .00f, .0f, 0, 1}       // three translations of 0.0
        };
        private static readonly ImageAttributes imageAttributes = new ImageAttributes();
        private static readonly ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
        static Form1()
        {
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }

        private Bitmap bmp = new Bitmap(WIDTH, HEIGHT), bmp2;
        private Graphics g;
        private List<Tuple<PointF, byte>> points = new List<Tuple<PointF, byte>>();
        private UdpClient client = new UdpClient(Constants.PC_PORT);
        private bool initiated = false;

        public Form1()
        {
            InitializeComponent();
            this.ClientSize = DEFAULT_SIZE;
            this.BackgroundImage = bmp;
#if MULTIPOINTS
            last = GetPoints(GetRandomPoint());
#else
            last = GetRandomPoint();
#endif
            g = Graphics.FromImage(bmp);
            g.Clear(BG_COLOR);
            timer.Interval = SLEEP;
            timer.Start();

            this.DoubleBuffered = true;

            new Thread(UDPThread).Start();
            new Thread(PainThread).Start();
        }

        private void UDPThread()
        {
            Thread.CurrentThread.IsBackground = true;

            while (true)
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] buf = client.Receive(ref remote);
                initiated = true;
                PointF p = new PointF(BitConverter.ToSingle(buf, 1) * SCALE, BitConverter.ToSingle(buf, sizeof(float) + 1) * SCALE);
                Tuple<PointF, byte> tup = new Tuple<PointF, byte>(p, buf[0]);
                lock (this)
                {
                    points.Add(tup);
                }
            }
        }

#if MULTIPOINTS
        private PointF[] last = GetPoints(GetRandomPoint());
#else
        private PointF last = GetRandomPoint();
#endif

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime d1 = DateTime.Now;
            List<Tuple<PointF, byte>> tmp;
            lock (this)
            {
                tmp = points;
                points = new List<Tuple<PointF, byte>>();
            }
            bmp2 = new Bitmap(WIDTH, HEIGHT);
            Graphics g2 = Graphics.FromImage(bmp2);
            g2.Clear(BG_COLOR);
            g2.DrawImage(bmp, DEFAULT_RECTANGLE, 0, 0, WIDTH, HEIGHT, GraphicsUnit.Pixel, imageAttributes);
            foreach (var p in tmp)
            {
#if MULTIPOINTS
                PointF[] n = GetPoints(p.Item1);
                if (p.Item2 != 0)
                    for (int i = 0; i < n.Length; i++)
                        g2.DrawLine(LINE_PEN, last[i], n[i]);
#else
                PointF n = p;
                g2.DrawLine(LINE_PEN, last, n);
#endif
                last = n;
            }
            g.DrawImage(bmp2, 0, 0);

            DateTime d2 = DateTime.Now;
            label1.Text = string.Format("{0:0.000}", d2.Subtract(d1).TotalMilliseconds);
            this.Invalidate();
            if (toSend == null)
            {
                toSend = bmp2;
                sem.Release();
            }
        }

        private Bitmap toSend = null;
        private Semaphore sem = new Semaphore(0, 1);
        private void PainThread()
        {
            Thread.CurrentThread.IsBackground = true;
            //Thread.Sleep(10000);
            byte id = 0;
            byte[] buf = new byte[PKT_SIZE];
            while (true)
            {
                sem.WaitOne();

                if (initiated)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        toSend.Save(ms, ImageFormat.Png);
                        int size = (int)ms.Seek(0, SeekOrigin.Current), pos = 0;
                        buf[0] = id++; pos++;
                        byte[] tmp = BitConverter.GetBytes(size);
                        Array.Copy(tmp, 0, buf, pos, tmp.Length); pos += tmp.Length;
                        ms.Seek(0, SeekOrigin.Begin);

                        int read = ms.Read(buf, pos, Math.Min(PKT_SIZE - pos, size));
                        size -= read; pos += read;
                        client.Send(buf, pos, Constants.PHONE_ADDR);

                        while (size > 0)
                        {
                            buf[0] = id; pos = 1;
                            tmp = BitConverter.GetBytes(size);
                            Array.Copy(tmp, 0, buf, pos, tmp.Length); pos += tmp.Length;
                            read = ms.Read(buf, pos, Math.Min(PKT_SIZE - pos, size));
                            size -= read; pos += read;
                            client.Send(buf, pos, Constants.PHONE_ADDR);
                        }
                        id++;
                    }
                }

                toSend = null;
                //    Console.WriteLine("{0}", count++);
            }
        }


        private static Random rand = new Random(0);
        private static PointF GetRandomPoint()
        {
            return new PointF(rand.Next(WIDTH), rand.Next(HEIGHT));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            g.Clear(BG_COLOR);
        }

        private static PointF Rotate(PointF p, PointF relative, double radius)
        {
            return new PointF((float)((p.X - relative.X) * Math.Cos(radius) - (p.Y - relative.Y) * Math.Sin(radius) + relative.X),
                (float)((p.X - relative.X) * Math.Sin(radius) + (p.Y - relative.Y) * Math.Cos(radius) + relative.Y));
        }

        private static PointF[] GetPoints(PointF p)
        {
            PointF[] ret = new PointF[COPIES];
            ret[0] = p;
            for (int i = 1; i < COPIES; i++)
            {
                ret[i] = Rotate(p, CENTER, Math.PI * 2 / COPIES * i);
            }
            return ret;
        }

    }
}
