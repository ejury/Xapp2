using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Xapp2.Models
{
     class Globals
    {
        // Tracking UI Display Variables
        public static bool init = false;
        public static string unit = "NA";
        public static string vessel = "NA";

        // Tracking Logged in user details
        public static int SELevel = 0;
        public static string UserDisplay = null;

        // WebAPI Variables
        public static HttpClient client = new HttpClient();
        public static string JWTkey = null;

        //Temporary for development
        public static int NFCtempcount = 100;

    }
}
