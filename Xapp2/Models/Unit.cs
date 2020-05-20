using System;
using SQLite;
using System.Collections.Generic;
using System.Text;

namespace Xapp2.Models
{
    public class Unit
    {
        [PrimaryKey, AutoIncrement]
        public int UnitID { get; set; }
        public string Name { get; set; }
     //   private ICollection<Vessel> Vessels { get; set; }
    }
}
