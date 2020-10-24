using System;
using SQLite;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Xapp2.Models;
using System.Data.SqlClient;
using System.Data;

namespace Xapp2.Data
{
    public class XDatabase
    {
        private SQLiteAsyncConnection _connection;

        public XDatabase(string dbPath)
        {
            //  _connection = DependencyService.Get<ISQLiteInterface>().GetConnection();
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<Unit>().Wait();
            _connection.CreateTableAsync<Vessel>().Wait();
            _connection.CreateTableAsync<Worker>().Wait();
            _connection.CreateTableAsync<EntryLog>().Wait();
            _connection.CreateTableAsync<AnalyticsLog>().Wait();
            _connection.CreateTableAsync<LoginViewModel>().Wait();
        }

        //Pull table lists from database
        public Task<List<Worker>> GetWorkers()
        {
            return _connection.Table<Worker>().ToListAsync();
        }
         async public Task<List<Worker>> GetWorkersAPI() //Get workerrequest forcing update from database
        {
            //Determine max ID already pulled from server
            var internalworkers = _connection.Table<Worker>().ToListAsync().Result;
            int maxID;
            try {   maxID = internalworkers.Select(c => c.WorkerID).Max();  }
            catch { maxID = 0; }


            //Pull only new entries from server
            IEnumerable<Worker> tempW = await APIServer.GetAllWorkers(maxID.ToString());

            await _connection.InsertAllAsync(tempW); //insert database entries into local db (if any)
            return await _connection.Table<Worker>().ToListAsync();

        }
        public Task<List<Vessel>> GetVessels()
        {
            return _connection.Table<Vessel>().ToListAsync();
        }
        async public Task<List<Vessel>> GetVesselsAPI() //Get workerrequest forcing update from database
        {
            //Determine max ID already pulled from server
            var internalvessels = _connection.Table<Vessel>().ToListAsync().Result;
            int maxID;
            try { maxID = internalvessels.Select(c => c.VesselID).Max(); }
            catch { maxID = 0; }

            //Pull only new entries from server
            IEnumerable<Vessel> tempV = await APIServer.GetAllVessels(maxID.ToString());

            await _connection.InsertAllAsync(tempV); //insert database entries into local db (if any)
            return await _connection.Table<Vessel>().ToListAsync();

        }

        public Task<List<Unit>> GetUnits()
        {
            return _connection.Table<Unit>().ToListAsync();
        }
        async public Task<List<Unit>> GetUnitsAPI() //Get workerrequest forcing update from database
        {
            //Determine max ID already pulled from server
            var internalunits = _connection.Table<Unit>().ToListAsync().Result;
            int maxID;
            try { maxID = internalunits.Select(c => c.UnitID).Max(); }
            catch { maxID = 0; }

            //Pull only new entries from server
            IEnumerable<Unit> tempU = await APIServer.GetAllUnits(maxID.ToString());

            await _connection.InsertAllAsync(tempU); //insert database entries into local db (if any)
            return await _connection.Table<Unit>().ToListAsync();

        }
        public Task<List<EntryLog>> GetLogs()
        {
            return _connection.Table<EntryLog>().ToListAsync();
        }
        async public Task<List<EntryLog>> GetLogsAPI() //Get workerrequest forcing update from database
        {
            //Determine max ID already pulled from server
            var internallogs = _connection.Table<EntryLog>().ToListAsync().Result;
            int maxID;
            try { maxID = internallogs.Select(c => c.EntryID).Max(); }
            catch { maxID = 0; }

            //Pull only new entries from server
            IEnumerable<EntryLog> tempE = await APIServer.GetAllEntryLogs(maxID.ToString());

            await _connection.InsertAllAsync(tempE); //insert database entries into local db (if any)
            return await _connection.Table<EntryLog>().ToListAsync();

        }
        public Task<List<AnalyticsLog>> GetAnalytics()
        {
            return _connection.Table<AnalyticsLog>().ToListAsync();
        }
        async public Task<List<AnalyticsLog>> GetAnalyticsAPI() //Get workerrequest forcing update from database
        {
            //Determine max ID already pulled from server
            var internallogs = _connection.Table<AnalyticsLog>().ToListAsync().Result;
            int maxID;
            try { maxID = internallogs.Select(c => c.EntryID).Max(); }
            catch { maxID = 0; }

            //Pull only new entries from server
            IEnumerable<AnalyticsLog> tempA = await APIServer.GetAllAnalyticsLogs(maxID.ToString());

            await _connection.InsertAllAsync(tempA); //insert database entries into local db (if any)
            return await _connection.Table<AnalyticsLog>().ToListAsync();

        }

        //Pull individual IDs from database list
        public Task<Vessel> GetVessel(int ident)
        {
            return _connection.Table<Vessel>().FirstOrDefaultAsync(t => t.VesselID == ident);
        }
        public Worker GetWorker(int ident)
        {
            return _connection.Table<Worker>().FirstOrDefaultAsync(t => t.WorkerID == ident).Result;
            
        }

        //Delete individual IDs from database list
        public Task<int> DeleteUnit(Unit unit)
        {
            APIServer.Delete("U" + unit.UnitID);
            return _connection.DeleteAsync<Unit>(unit.UnitID);
        }
        public Task<int> DeleteVessel(int ident)
        {
            APIServer.Delete("V" + ident);
            return _connection.DeleteAsync<Vessel>(ident);
        }
        public Task<int> DeleteLog(int ident)
        {
            APIServer.DeleteLog(ident);
            return _connection.DeleteAsync<EntryLog>(ident);
        }
        public Task<int> DeleteWorker(int ident, string NFCtag)
        {
            APIServer.DeactivateWorker(NFCtag);

            //Update worker to show not activated to ensure filtered out of worker page list
            Worker tempworker = GetWorker(ident);
            tempworker.Activated = 0;
            return _connection.UpdateAsync(tempworker);
        }


        //Add Individual Entries to database list
        async public Task<int> AddWorker(Worker worker)
        {
            //Determine max ID already pulled from server
            var internalcount = _connection.Table<Worker>().ToListAsync().Result;
            int maxID;
            if(internalcount.Count == 0)
            { maxID = 0; }
            else 
            { maxID = internalcount.Select(c => c.WorkerID).Max(); }
             

            //Add worker and retrieve assigned ID
            int newID = await APIServer.AddWorker(worker); //Add worker

            //No query to database for missing workers as this will be handled during app load or when new user is swiped

            if (newID == maxID + 1) //New ID reports of -1 (invalid card) and 0 (already activated card) will not be updated locally or in SQL Db
            {
                //Add worker to local database
                worker.WorkerID = newID;
                if (worker.ReferenceNFC == Globals.ServerName + "_V")
                {
                    worker.ReferenceNFC = worker.ReferenceNFC + newID;
                }
                await _connection.InsertAsync(worker);
            }
            if (newID > maxID + 1) //Missing entries added by another user
            {
                IEnumerable<Worker> tempW = await APIServer.GetAllWorkers(maxID.ToString());

                await _connection.InsertAllAsync(tempW);
            }
            return newID;
        }
        async public Task<int> AddVessel(Vessel vessel)
        {
            //Determine max ID already pulled from server
            var internalcount = _connection.Table<Vessel>().ToListAsync().Result;
            int maxID;
            if (internalcount.Count == 0)
            { maxID = 0; }
            else
            { maxID = internalcount.Select(c => c.VesselID).Max(); }

            //Add vessel and retrieve assigned ID
            int newID = await APIServer.AddVessel(vessel);

            if (newID == maxID+1) //New ID reports of -1 (unsuccessful SQL) and 0 (incorrect API request) will not be updated locally or in SQL Db
            {
                //Add worker to local database
                vessel.VesselID = newID;
                await _connection.InsertAsync(vessel);
            }
            if (newID > maxID + 1) //Missing entries added by another user
            {
                IEnumerable<Vessel> tempV = await APIServer.GetAllVessels(maxID.ToString());

                await _connection.InsertAllAsync(tempV);
            }
          
            return newID;
        }
        async public Task<int> AddUnit(Unit unit)
        {
            //Determine max ID already pulled from server
            var internalcount = _connection.Table<Unit>().ToListAsync().Result;
            int maxID;
            if (internalcount.Count == 0)
            { maxID = 0; }
            else
            { maxID = internalcount.Select(c => c.UnitID).Max(); }

            //Add unit and retrieve assigned ID
            int newID = await APIServer.AddUnit(unit);

            if (newID == maxID + 1) //New ID reports of -1 (unsuccessful SQL) and 0 (incorrect API request) will not be updated locally or in SQL Db
            {
                //Add worker to local database
                unit.UnitID = newID;
                await _connection.InsertAsync(unit);
            }
            if (newID > maxID + 1) //Missing entries added by another user
            {
                IEnumerable<Unit> tempU = await APIServer.GetAllUnits(maxID.ToString());

/*                _connection.DropTableAsync<Unit>().Wait();
                _connection.CreateTableAsync<Unit>().Wait();*/

                await _connection.InsertAllAsync(tempU);
            }

            return newID;
        }  
        async public Task<int> AddLog(EntryLog entrylog)
        {
            int maxID; int minID; //minID for offline mode -ID creation
            //Determine max ID already pulled from server
            var internalcount = _connection.Table<EntryLog>().ToListAsync().Result;

            if (internalcount.Count == 0)
            { maxID = 0; minID = 0; }
            else
            {
                maxID = internalcount.Select(c => c.EntryID).Max();
                minID = internalcount.Select(c => c.EntryID).Min();
                if (minID > 0) { minID = -1; }
            }

            if (Globals.OfflineMode == false)
            {
                //Add unit and retrieve assigned ID
                int newID = await APIServer.AddLog(entrylog);

                if (newID == maxID + 1) //New ID is provided by Db, no error handling similar to worker flags.
                {
                    //Add worker to local database
                    entrylog.EntryID = newID;
                    await _connection.InsertAsync(entrylog);
                }
                if (newID > maxID + 1) //Missing entries added by another user
                {
                    IEnumerable<EntryLog> tempE = await APIServer.GetAllEntryLogs(maxID.ToString());
                    await _connection.InsertAllAsync(tempE);
                }
                return newID;
            }
            else
            {
                //For offline mode, do not update online db and use negative ID#s
                entrylog.EntryID = minID-1;
                await _connection.InsertAsync(entrylog);
                return 1;
            }

            
        }
        async public Task<int> AddAnalyticsLog(AnalyticsLog analyticlog)
        {
            if (Globals.OfflineMode == false)
            {
                //Determine max ID already pulled from server
                var internalcount = _connection.Table<AnalyticsLog>().ToListAsync().Result;
                int maxID;
                if (internalcount.Count == 0)
                { maxID = 0; }
                else
                { maxID = internalcount.Select(c => c.EntryID).Max(); }

                //Add unit and retrieve assigned ID
                int newID = await APIServer.AddRecord(analyticlog);

                if (newID == maxID + 1) //New ID is provided by Db, no error handling similar to worker flags.
                {
                    //Add worker to local database
                    analyticlog.EntryID = newID;
                    await _connection.InsertAsync(analyticlog);
                }
                if (newID > maxID + 1) //Missing entries added by another user
                {
                    IEnumerable<EntryLog> tempE = await APIServer.GetAllEntryLogs(maxID.ToString());
                    await _connection.InsertAllAsync(tempE);
                }

                return newID;
            }
            else
            {
                analyticlog.EntryID = -1;
                await _connection.InsertAsync(analyticlog);
                return 1;
            }
        }

        async public Task<int> SyncLogs(List<AnalyticsLog> analyticslist)
        {
            int newID = await APIServer.SyncLogs(analyticslist);

            if (newID > 0)
            {
                _connection.DropTableAsync<EntryLog>().Wait();
                _connection.CreateTableAsync<EntryLog>().Wait();
                _connection.DropTableAsync<AnalyticsLog>().Wait();
                _connection.CreateTableAsync<AnalyticsLog>().Wait();
                GetLogsAPI();
                GetAnalyticsAPI();
            }
            return newID;
        }


            //Remove entire database tables
            public Task ClearLocal()
        {
            _connection.DropTableAsync<Unit>().Wait();
            _connection.CreateTableAsync<Unit>().Wait();
            _connection.DropTableAsync<Vessel>().Wait();
            _connection.CreateTableAsync<Vessel>().Wait();
            _connection.DropTableAsync<Worker>().Wait();
            _connection.CreateTableAsync<Worker>().Wait();
            _connection.DropTableAsync<EntryLog>().Wait();
            _connection.CreateTableAsync<EntryLog>().Wait();
            _connection.DropTableAsync<AnalyticsLog>().Wait();
            _connection.CreateTableAsync<AnalyticsLog>().Wait();
            return Task.CompletedTask;
        }
        public Task ClearUnit()
        {
            APIServer.Delete("UxClearx");
            _connection.DropTableAsync<Unit>().Wait();
            _connection.CreateTableAsync<Unit>().Wait();
            return Task.CompletedTask;
        }
        public Task ClearVessel()
        {
            APIServer.Delete("VxClearx");
            _connection.DropTableAsync<Vessel>().Wait();
            _connection.CreateTableAsync<Vessel>().Wait();
            return Task.CompletedTask;
        }
        public Task ClearWorker()
        {
            APIServer.Delete("WxClearx");
            _connection.DropTableAsync<Worker>().Wait();
            _connection.CreateTableAsync<Worker>().Wait();
            Globals.NFCtempcount = 100;
            return Task.CompletedTask;
        }
        public Task ClearLogs()
        {
            APIServer.Delete("LxClearx");
            _connection.DropTableAsync<EntryLog>().Wait();
            _connection.CreateTableAsync<EntryLog>().Wait();
            return Task.CompletedTask;
        }
        public Task ClearAnalytics()
        {
            APIServer.Delete("RxClearx");
            _connection.DropTableAsync<AnalyticsLog>().Wait();
            _connection.CreateTableAsync<AnalyticsLog>().Wait();
            return Task.CompletedTask;
        }

        //Refresh and Seed entire database
        async public Task RefreshDatabase()
        {
            Unit unit = new Unit();
            Vessel vessel = new Vessel();
            Worker worker = new Worker();
            EntryLog newlog = new EntryLog();
            AnalyticsLog Alog = new AnalyticsLog();
            Random rnd = new Random();

            //Add list of workers
            List<string> workerfirst = new List<string> { "Carl", "Sam", "Nisbit", "Johnny", "Earl", "Pete", "Douglas", "Cara", "Avril", "Betty-Sue", "Dougy", "Alex", "Tom", "Tara", "Indy", "Brad", "Stu","Eddy", "Nero", "Carl", "Sam", "Nisbit", "Johnny", "Earl", "Pete", "Douglas", "Cara", "Avril", "Betty-Sue", "Dougy", "Alex", "Tom", "Tara", "Indy", "Brad", "Stu", "Eddy", "Nero" };
            List<string> workerlast = new List<string> { "Johnson", "Black", "Trout", "Smith", "Rogers", "Jury", "Slick", "Salty", "Hardy", "Bolton", "Carter", "Silversmith", "Snake", "Black", "White", "Wattson", "Ericson", "Edge", "Winter", "Ruderson-Jergen","Johnson", "Black", "Trout", "Smith", "Rogers", "Jury", "Slick", "Salty", "Hardy", "Bolton", "Carter", "Silversmith", "Snake", "Black", "White", "Wattson", "Ericson", "Edge" };
            List<string> companyname = new List<string> { "IOL", "IOL", "IOL", "IOL", "CEDA", "CEDA", "TEAM", "TEAM", "TEAM", "TAMS", "TAMS", "TAMS", "TAMS", "Curren", "Curren", "Curren","Safway", "Safway", "Safway", "IOL", "IOL", "IOL", "IOL", "CEDA", "CEDA", "TEAM", "TEAM", "TEAM", "TAMS", "TAMS", "TAMS", "TAMS", "Curren", "Curren", "Curren", "Safway", "Safway", "Safway" };
            List<string> nfclist = new List<string> { "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "112", "113", "114", "115", "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130", "131", "132", "133", "134", "135", "136", "137", "138" };
            string DB = "QQQ";
            for (int i = 0; i <= workerfirst.Count - 1; i++)
            {
                worker.FirstName = workerfirst[i];
                worker.LastName = workerlast[i];
                worker.Company = companyname[i];
                worker.CreatedTime = DateTime.Now;
                worker.Activated = 1;
                Globals.NFCtempcount++;
                worker.ReferenceNFC = DB + "_" + nfclist[i];
                await AddWorker(worker);

            }

            string[] unitname = new string[]  { "Office Building","Plant 1","Construction Site"};
            foreach (string t in unitname)
            {
                //Add Unit from unitname list
                unit.Name = t;
                await App.Database.AddUnit(unit);

                //Add a random vessel tag for each unit (x3-6)
                int VperU = rnd.Next(3, 7);
                for (int i = 1; i <= VperU; i++)
                {
                    int VorT = (int)rnd.Next(1, 3);
                    string Vtype;
                    if (VorT == 1)
                    { Vtype = "Entrance"; }
                    else
                    { Vtype = "Checkpoint"; }
                    int Vnum = rnd.Next(100, 400);
                    vessel.Name = Vtype + "-" + Vnum;
                    vessel.Unitname = t;
                    await AddVessel(vessel);

                    //50% Chance for active entry to be present on added vessel
                    int status = rnd.Next(0, 2);
                    if (status==1)
                    {
                        int Logs = 6;
                        int EntriesV = rnd.Next(0, Logs+1); //Random number of entries per vessel

                        //Create worker name temp lists to allow temporary removal as 'workers enter vessels'
                        List<string> Ctemp = new List<string>(); Ctemp.AddRange(companyname);
                        List<string> Ftemp = new List<string>(); Ftemp.AddRange(workerfirst);
                        List<string> Ltemp = new List<string>(); Ltemp.AddRange(workerlast);
                        List<string> Ntemp = new List<string>(); Ntemp.AddRange(nfclist);
                        for (int k = 0; k < EntriesV; k++)
                        {
                            //Randomize worker selection and datetime stamp
                            int W = rnd.Next(0, workerfirst.Count-k);
                            int timestamp = rnd.Next(1, 10);

/*                            //Create workerID Redundent due to NFC tie to worker
                            newlog.WorkerID = W+1;
                            newlog.Company = Ctemp[W];
                            newlog.FirstName = Ftemp[W];
                            newlog.LastName = Ltemp[W];*/
                            newlog.ReferenceNFC = DB + "_" + Ntemp[W];
                            newlog.TimeLog = DateTime.Now.AddHours(-timestamp);
                            newlog.VesselName = vessel.Name;
                            newlog.UnitName = vessel.Unitname;
                            await AddLog(newlog);

                            //Create analytics log (z days of data)
                            for (int z=1; z < 4; z++)
                            {
                                //adding time randomizers
                                int xhour = rnd.Next(1, 8);
                                int xhour2 = rnd.Next(1, 8);
                                int xmin = rnd.Next(0, 59);
                                int xmin2 = rnd.Next(0, 59);
                                DateTime xtime = new DateTime();

                                //Create entry log
                                Alog.ReferenceNFC = DB + "_" + Ntemp[W];
                                Alog.InOut = 1;
                                    xtime = DateTime.Now.AddHours(-(24*z+xhour));
                                    xtime = xtime.AddMinutes(-xmin);
                                Alog.TimeLog = xtime;
                                Alog.VesselName = vessel.Name;
                                Alog.UnitName = vessel.Unitname;
                                await APIServer.AddRecord(Alog);

                                //create exit log
                                Alog.ReferenceNFC = DB + "_" + Ntemp[W];
                                Alog.InOut = -1;
                                     xtime = DateTime.Now.AddHours(-(24 * z - xhour2)); 
                                     xtime = xtime.AddMinutes(+xmin2);
                                Alog.TimeLog = xtime;
                                Alog.VesselName = vessel.Name;
                                Alog.UnitName = vessel.Unitname;
                                await APIServer.AddRecord(Alog);
                            }

                            // matching most recent entry of  Entrylog
                            Alog.ReferenceNFC = DB + "_" + Ntemp[W];
                            Alog.InOut = 1;
                            Alog.TimeLog = DateTime.Now.AddHours(-timestamp);
                            Alog.VesselName = vessel.Name;
                            Alog.UnitName = vessel.Unitname;
                            await APIServer.AddRecord(Alog);

                            //Removing worker from list to avoid duplication
                            Ctemp.RemoveAt(W); Ftemp.RemoveAt(W); Ltemp.RemoveAt(W); Ntemp.RemoveAt(W);
                            
                        }
                        //Populate analytics list with database
                        IEnumerable<AnalyticsLog> tempA = await APIServer.GetAllAnalyticsLogs("0");

                        _connection.DropTableAsync<AnalyticsLog>().Wait();
                        _connection.CreateTableAsync<AnalyticsLog>().Wait();
                        await _connection.InsertAllAsync(tempA);
                    }
                }
            }

            return;// Task.CompletedTask;
        }
    }
}
