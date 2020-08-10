using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xapp2.Models
{
    public class EntryLog
    {
        [PrimaryKey]
        public int EntryID { get; set; }
        

        // Location Details
        public string VesselName {get; set;}
        public string UnitName { get; set; }
        public string ReferenceNFC { get; set; }

/*        //User Details Redundent due to NFC tie to worker
        public int WorkerID { get; set; }
        public string Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }*/
        // 1=enter, 0=exit

        //public int InOut { get; set; } phased out
        public DateTime TimeLog { get; set; }

    }
}
