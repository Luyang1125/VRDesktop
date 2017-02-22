using System;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using Android.App;
using Android.Content;
using Android.Gestures;
using Android.Graphics;
using Android.Runtime;
using Android.Net;
using Android.Media;
using Android.Net.Rtp;
using Android.Views;
using Android.Widget;
using Android.OS;
using Common;
using Android.Util;
using System.Threading;


namespace Test1
{
    [Activity(Label = "Test1", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, View.IOnTouchListener
    {
        private class UDPImg
        {
            private static int HEADER_SIZE = 1 + sizeof(int);
            private int remainingSize = -1;
            private byte[] buf = null;
            public byte[] PutFirstContent(byte[] first)
            {
                remainingSize = BitConverter.ToInt32(first, 1);
                buf = new byte[remainingSize];
                int payloadSize = first.Length - HEADER_SIZE;
                Array.Copy(first, HEADER_SIZE, buf, buf.Length - remainingSize, payloadSize);
                remainingSize -= payloadSize;
                return remainingSize == 0 ? buf : null;
            }

            public byte[] PutContent(byte[] next)
            {
                int pktRemaining = BitConverter.ToInt32(next, 1);
                int payloadSize = next.Length - HEADER_SIZE;
                Array.Copy(next, HEADER_SIZE, buf, buf.Length - pktRemaining, payloadSize);
                remainingSize -= payloadSize;
                if (remainingSize == 0)
                {
                    Log.Debug("Test1", "img len: " + buf.Length);
                    return buf;
                }
                return null;
            }
        }


        private UdpClient client = new UdpClient(Constants.PHONE_PORT);
        //private Button btn = null;
        private ImageView imgView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


            // Get our button from the layout resource,
            // and attach an event to it
            //btn = FindViewById<Button>(Resource.Id.MyButton);
            imgView = FindViewById<ImageView>(Resource.Id.imageView1);
            imgView.SetOnTouchListener(this);

            //imgView.SetImageBitmap()


            new Thread(UDPThread).Start();
        }

        private void SetImage(byte[] img)
        {
            Bitmap bmp = BitmapFactory.DecodeByteArray(img, 0, img.Length);
            //Log.Debug("Test1", "len={0}", imgBuf.Length);
            if (imgView != null)
            {
                RunOnUiThread(() =>
                {
                    imgView.SetImageBitmap(bmp);
                });
            }
            System.GC.Collect();
        }

        public void UDPThread()
        {
            Thread.CurrentThread.IsBackground = true;

            UDPImg lastImg = null, currImg = null;
            int lastId = -1, currId = -1;
            while (true)
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] buf = client.Receive(ref remote);
                byte id = buf[0];
                if (id % 2 == 0)
                {
                    if (lastImg != null)
                        Log.Debug("Test1", "Drop frame: {0}", lastId);
                    lastImg = currImg;
                    lastId = currId;

                    currImg = new UDPImg();
                    currId = id + 1;
                    byte[] ret = currImg.PutFirstContent(buf);
                    if (ret != null)
                    {
                        SetImage(ret);
                        lastId = currId = -1;
                        lastImg = currImg = null;
                    }
                }
                else if (id == lastId)
                {
                    byte[] ret = lastImg.PutContent(buf);
                    if (ret != null)
                    {
                        SetImage(ret);
                        lastId = -1;
                        lastImg = null;
                    }
                }
                else if (id == currId)
                {
                    byte[] ret = currImg.PutContent(buf);
                    if (ret != null)
                    {
                        SetImage(ret);
                        lastId = currId = -1;
                        lastImg = currImg = null;
                    }
                }
                else
                {
                    //discard
                }

            }



            //    int expectId = -1, expectSize = -1;
            //bool imageFailed = true;
            //byte[] imgBuf = null;
            //while (true)
            //{
            //    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            //    byte[] buf = client.Receive(ref remote);
            //    int pos = 0;
            //    byte id = buf[0]; pos++;
            //    if (id % 2 == 0)
            //    {
            //        int len = BitConverter.ToInt32(buf, 1); pos += sizeof(int);
            //        imgBuf = new byte[len];
            //        expectSize = len - buf.Length + pos;
            //        if (expectSize < 0) { imageFailed = true; continue; }
            //        Array.Copy(buf, pos, imgBuf, 0, buf.Length - pos);
            //        if (expectSize > 0)
            //        {
            //            imageFailed = false;
            //            expectId = id + 1;
            //        }
            //    }
            //    else if (imageFailed || expectId != id) continue;
            //    else
            //    {
            //        int len = BitConverter.ToInt32(buf, 1); pos += sizeof(int);
            //        expectSize -= buf.Length - pos;
            //        if (expectSize < 0) { imageFailed = true; continue; }
            //        Array.Copy(buf, pos, imgBuf, imgBuf.Length - len, buf.Length - pos);
            //    }
            //    if (expectSize == 0)
            //    {
            //        Bitmap bmp = BitmapFactory.DecodeByteArray(imgBuf, 0, imgBuf.Length);
            //        //Log.Debug("Test1", "len={0}", imgBuf.Length);
            //        if (imgView != null)
            //        {
            //            RunOnUiThread(() =>
            //            {
            //                imgView.SetImageBitmap(bmp);
            //            });
            //        }
            //        System.GC.Collect();
            //        imageFailed = true;
            //    }


            //Rect rect = new Rect();
            //imgView.GetLocalVisibleRect(rect);
            //Log.Debug("Test1", string.Format("{0},{1}", rect.Width(), rect.Height()));


        }

        public bool OnTouch(View v, MotionEvent e)
        {
            //Log.Debug("Test", "{0},{1} {2}", e.GetX(), e.GetY(), e.Action);
            byte[] bufx = BitConverter.GetBytes(e.GetX()),
                    bufy = BitConverter.GetBytes(e.GetY());
            byte[] buf = new byte[bufx.Length + bufy.Length + 1];
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    buf[0] = 0;
                    break;
                case MotionEventActions.Move:
                    buf[0] = 1;
                    break;
                //case MotionEventActions.Up:
                default:
                    buf[0] = 2;
                    break;
            }
            Array.Copy(bufx, 0, buf, 1, bufx.Length);
            Array.Copy(bufy, 0, buf, 1 + bufx.Length, bufy.Length);

            client.Send(buf, buf.Length, Constants.PC_ADDR);
            return true;
        }
    }
}

