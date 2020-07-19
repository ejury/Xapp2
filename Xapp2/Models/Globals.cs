using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Xapp2.Models
{
     class Globals
    {
        public static bool init = false;

        public static string unit = "NA";

        public static string vessel = "NA";

        public static string AKey = null;

        public static HttpClient client = new HttpClient();

        public static string JWTkey = null;

        public static int NFCtempcount = 100;

    }
}
