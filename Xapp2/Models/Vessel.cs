using System;
using SQLite;
using System.Collections.Generic;
using System.Text;


namespace Xapp2.Models
{
    public class Vessel
    {
        //[PrimaryKey, AutoIncrement]
        [PrimaryKey]
        public int VesselID { get; set; }
       // public int UnitID { get; set; }
        public string Name { get; set; }

        public string Unitname { get; set; }
       // public ICollection<Worker> Workers { get; set; }
    }
}
