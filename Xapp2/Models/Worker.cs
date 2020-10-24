using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Xapp2.Models
{
   public  class Worker
    {
        [PrimaryKey]
        public int WorkerID { get; set; }
        public string ReferenceNFC { get; set; }
        public string LastName { get; set; }
        
        public string FirstName { get; set; }

        public string Company { get; set; }

        public int SELevel { get; set; }

        public int Activated { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
