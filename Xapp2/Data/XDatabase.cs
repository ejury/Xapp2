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
        public Task<List<Vessel>> GetVessels()
        {
            return _connection.Table<Vessel>().ToListAsync();
        }
        public Task<List<Unit>> GetUnits()
        {

            return _connection.Table<Unit>().ToListAsync();
        }
        public Task<List<EntryLog>> GetLogs()
        {

            return _connection.Table<EntryLog>().ToListAsync();
        }
        public Task<List<AnalyticsLog>> GetAnalytics()
        {

            return _connection.Table<AnalyticsLog>().ToListAsync();
        }

        //Pull individual IDs from database list
        public Task<Vessel> GetVessel(int ident)
        {
            return _connection.Table<Vessel>().FirstOrDefaultAsync(t => t.VesselID == ident);
        }
        public Task<Worker> GetWorker(int ident)
        {
            return _connection.Table<Worker>().FirstOrDefaultAsync(t => t.WorkerID == ident);
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
            return _connection.DeleteAsync<EntryLog>(ident);
        }

        //Add Individual Entries to database list
        async public Task<int> AddWorker(Worker worker)
        {
            await APIServer.Add(new Tuple<Unit, Vessel, Worker>(null, null, worker), "W");
            IEnumerable<Worker> tempW = await APIServer.GetAllWorkers();

            _connection.DropTableAsync<Worker>().Wait();
            _connection.CreateTableAsync<Worker>().Wait();


            await  _connection.InsertAllAsync(tempW);
            int t = 1;
            return t;
        }
        async public Task<int> AddVessel(Vessel vessel)
        {

            await APIServer.Add(new Tuple<Unit, Vessel, Worker>(null, vessel, null), "V");
            IEnumerable<Vessel> tempV = await APIServer.GetAllVessels();

            _connection.DropTableAsync<Vessel>().Wait();
            _connection.CreateTableAsync<Vessel>().Wait();
            
            await    _connection.InsertAllAsync(tempV);
            int t = 1;
            return t;
        }
        async public Task<int> AddUnit(Unit unit)
        {

            await APIServer.Add(new Tuple <Unit, Vessel, Worker> (unit,null,null), "U");
            IEnumerable<Unit> tempU = await APIServer.GetAllUnits();

            _connection.DropTableAsync<Unit>().Wait();
            _connection.CreateTableAsync<Unit>().Wait();

            await _connection.InsertAllAsync(tempU);
            int t = 1;
            return t;
        }
        public Task<int> AddLog(EntryLog entrylog)
        {
            return _connection.InsertAsync(entrylog);

        }
        public Task<int> AddAnalyticsLog(AnalyticsLog entrylog)
        {
            return _connection.InsertAsync(entrylog);

        }

        //Remove entire database tables
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
            _connection.DropTableAsync<EntryLog>().Wait();
            _connection.CreateTableAsync<EntryLog>().Wait();
            return Task.CompletedTask;
        }
        public Task ClearAnalytics()
        {
            _connection.DropTableAsync<EntryLog>().Wait();
            _connection.CreateTableAsync<EntryLog>().Wait();
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
            List<string> workerfirst = new List<string> { "Carl", "Sam", "Nisbit", "Johnny", "Earl", "Pete", "Douglas", "Cara", "Avril", "Betty-Sue", "Dougy", "Alex", "Tom" };
            List<string> workerlast = new List<string> { "Johnson", "Black", "Trout", "Smith", "Rogers", "Jury", "Slick", "Salty", "Hardy", "Bolton", "Carter", "Silversmith", "Snake" };
            List<string> companyname = new List<string> { "IOL", "IOL", "IOL", "IOL", "CEDA", "CEDA", "TEAM", "TEAM", "TEAM", "TAMS", "TAMS", "TAMS", "TAMS" };

            for (int i = 1; i <= workerfirst.Count - 1; i++)
            {
                worker.FirstName = workerfirst[i];
                worker.LastName = workerlast[i];
                worker.Company = companyname[i];
                Globals.NFCtempcount++;
                worker.ReferenceNFC = Globals.NFCtempcount.ToString();
                await AddWorker(worker);

            }

            string[] unitname = new string[]  { "CCIS","GCIS","COB"};
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
                    { Vtype = "T"; }
                    else
                    { Vtype = "D"; }
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
                        for (int k = 0; k < EntriesV; k++)
                        {
                            //Randomize worker selection and datetime stamp
                            int W = rnd.Next(0, workerfirst.Count-k);
                            int timestamp = rnd.Next(0, 180);

                            //Create workerID
                            newlog.WorkerID = W+1;
                            newlog.Company = Ctemp[W];
                            newlog.FirstName = Ftemp[W];
                            newlog.LastName = Ltemp[W];
                            newlog.InOut = 1;
                            newlog.TimeLog = DateTime.Now.AddMinutes(-timestamp);
                            newlog.VesselName = vessel.Name;
                            newlog.UnitName = vessel.Unitname;
                            await AddLog(newlog);

                            //Create analytics log (z days of data)
                            for (int z=0; z < 3; z++)
                            {
                                //adding time randomizers
                                int xhour = rnd.Next(1, 4);
                                int xmin = rnd.Next(0, 60);
                                int xmin2 = rnd.Next(0, 60);
                                DateTime xtime = new DateTime();

                                //Create entry log
                                Alog.WorkerID = W + 1;
                                Alog.Company = Ctemp[W];
                                Alog.FirstName = Ftemp[W];
                                Alog.LastName = Ltemp[W];
                                Alog.InOut = 1;
                                    xtime = DateTime.Now.AddHours(-(24*z+xhour));
                                    xtime = xtime.AddMinutes(-xmin);
                                Alog.TimeLog = xtime;
                                Alog.VesselName = vessel.Name;
                                Alog.UnitName = vessel.Unitname;
                                await AddAnalyticsLog(Alog);

                                //create exit log
                                Alog.WorkerID = W + 1;
                                Alog.Company = Ctemp[W];
                                Alog.FirstName = Ftemp[W];
                                Alog.LastName = Ltemp[W];
                                Alog.InOut = 0;
                                     xtime = xtime.AddHours(xhour);
                                     xtime = xtime.AddMinutes(+xmin2);
                                Alog.TimeLog = xtime;
                                Alog.VesselName = vessel.Name;
                                Alog.UnitName = vessel.Unitname;
                                await AddAnalyticsLog(Alog);
                            }

                            // matching most recent entry of  Entrylog
                            Alog.WorkerID = W + 1;
                            Alog.Company = Ctemp[W];
                            Alog.FirstName = Ftemp[W];
                            Alog.LastName = Ltemp[W];
                            Alog.InOut = 1;
                            Alog.TimeLog = DateTime.Now.AddMinutes(-timestamp);
                            Alog.VesselName = vessel.Name;
                            Alog.UnitName = vessel.Unitname;
                            await AddAnalyticsLog(Alog);

                            //Removing worker from list to avoid duplication
                            Ctemp.RemoveAt(W); Ftemp.RemoveAt(W); Ltemp.RemoveAt(W);
                            
                        }
                    }
                }
            }

            return;// Task.CompletedTask;
        }
    }
}
