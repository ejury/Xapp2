 using System;
using System.Collections.Generic;
using System.Text;

namespace Xapp2.Models
{
    public class AnalyticsLog
    {
        public int EntryID { get; set; }
        public string VesselName { get; set; }
        public string UnitName { get; set; }
        public string ReferenceNFC { get; set; }

        // 1=enter, -1=exit
        public int InOut { get; set; }
        public DateTime TimeLog { get; set; }

/*        public string Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int WorkerID { get; set; }*/
    }
}
