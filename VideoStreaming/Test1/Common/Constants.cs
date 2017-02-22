using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;

namespace Common
{
    public static class Constants
    {
        public static readonly int PHONE_PORT = 10300;
        public static readonly int PC_PORT = 10301;
        public static readonly IPEndPoint PC_ADDR = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 42, 250 }), PC_PORT);
        public static readonly IPEndPoint PHONE_ADDR = new IPEndPoint(new IPAddress(new byte[] { 192, 168, 42, 129 }), PHONE_PORT);
        public static readonly int PHONE_RESOLUTION_WIDTH = 1080;
        public static readonly int PHONE_RESOLUTION_HEIGHT = 1584;
    }
}
