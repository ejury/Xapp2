using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;
using Xapp2.Data;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

        public partial class CSEntryPage : ContentPage
    { 
        public string currentunit;
        public string currentvessel;
        public bool InOutMode;
        bool Nav;
        List<EntryLog> currentworkers;
        EntryLog newlog = new EntryLog();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SetUnitList();
            InOutMode = InOutButton.IsToggled;
        }

        private async void SetUnitList()
        {
            var unitlistall = await App.Database.GetUnits();
            var unitlist = unitlistall.Select(c => c.Name).ToList();
            unitpicker.ItemsSource = unitlist;
            if (Globals.init)
            {
                currentunit = Globals.unit;
                currentvessel = Globals.vessel;
                unitpicker.SelectedItem = currentunit;
            }
        }
        private async void SetVesselList()
        {
            var vessellistall = await App.Database.GetVessels();
            var vessellistfilter = vessellistall.Where(w => w.Unitname == currentunit);
            List<string> vessellist = vessellistfilter.Select(c => c.Name).ToList();

            vesselpicker.ItemsSource = vessellist;
            if (Globals.init)
            { vesselpicker.SelectedItem = currentvessel; }
        }
        private async void SetWorkerList()
        {
            var workerlistall = await App.Database.GetWorkers();
            List<int> workerlist = workerlistall.Select(c => c.WorkerID).ToList();

            workerpicker.ItemsSource = workerlist;
        }
        private async void SetCSEList()
        {
            List<EntryLog> workerlistall = await App.Database.GetLogs();
            List<EntryLog> workerlist = workerlistall.Where(w => w.VesselName == currentvessel).ToList();
            List<TimeDisplay> timedisplay = new List<TimeDisplay>();
            //workerlist = workerlist.Where(w => w.InOut == 1);
            currentworkers = workerlist.ToList();
            CultureInfo format = new CultureInfo("en-US");

            for (int i = 0; i < workerlist.Count; i++)
            {
                TimeDisplay tempdisplay = new TimeDisplay();
                tempdisplay.DateLog = workerlist[i].TimeLog.ToString("dddd-dd", format);
                tempdisplay.TimeLog = workerlist[i].TimeLog.ToString("h:mm tt", format);
                tempdisplay.FirstName = workerlist[i].FirstName;
                tempdisplay.LastName = workerlist[i].LastName;
                tempdisplay.Company = workerlist[i].Company;
                timedisplay.Add(tempdisplay);
            }

            workersview.ItemsSource = timedisplay;

            var companylong = workerlist.Select(c => c.Company).ToList();
            var company = companylong.Distinct().ToList();
            int[] companycount = new int[company.Count];
            List<CompanyList> companylist = new List<CompanyList>();
            if (workerlist.Any() == true)
            {
                int iteration = 0;
                foreach (string i in company)
                {
                    int counter = 0;
                    foreach (string k in companylong)
                    {
                        if (i == k)
                        {
                            counter++;
                        }
                    }
                    companycount[iteration] = counter;
                    companylist.Add(new CompanyList { CompanyListName = company[iteration], CompanyListNum = companycount[iteration] });

                    iteration++;
                }
            }

            CompanyList.ItemsSource = companylist;
            int t = 1;
        }
        private async void InOutToggle(object sender, EventArgs e)
        {
            InOutMode = InOutButton.IsToggled;
        }
        public CSEntryPage()
        {
            Nav = true;
            InitializeComponent();
        }

        async void OnUnitPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                currentunit = (string)picker.ItemsSource[selectedIndex];
                SetVesselList();
                workerpicker.ItemsSource = null;
                workerpicker.Title = "Select Vessel";

                if (currentunit != Globals.unit)
                {
                    Globals.init = false; //Removing global item selection
                }
            }
        }
        async void OnVesselPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                currentvessel = (string)picker.ItemsSource[selectedIndex];
                workerpicker.Title = "Select Worker";
                SetWorkerList();
                SetCSEList();

                Globals.init = true; Globals.unit = currentunit; Globals.vessel = currentvessel; //Establishing global item selection
            }
        }
        async void OnWorkerPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;
            Worker worker = new Worker();

            if (selectedIndex != -1)
            {
                int selectedworker = (int)picker.ItemsSource[selectedIndex];

                if (InOutMode)
                {
                    var properworker = currentworkers.Where(w => w.WorkerID == selectedworker & w.InOut == 1);
                    int properworker2 = properworker.Count();
                    if (properworker2 == 0)
                    {
                        worker = await App.Database.GetWorker(selectedworker);

                        newlog.WorkerID = selectedworker;
                        newlog.Company = worker.Company;
                        newlog.FirstName = worker.FirstName;
                        newlog.LastName = worker.LastName;
                        newlog.InOut = 1;
                        newlog.TimeLog = DateTime.Now;
                        newlog.VesselName = currentvessel;
                        newlog.UnitName = currentunit;
                        AnalyticsLog Alog = new AnalyticsLog();
                        Alog.WorkerID = selectedworker;
                        Alog.Company = worker.Company;
                        Alog.FirstName = worker.FirstName;
                        Alog.LastName = worker.LastName;
                        Alog.InOut = 1;
                        Alog.TimeLog = DateTime.Now;
                        Alog.VesselName = currentvessel;
                        Alog.UnitName = currentunit;
                      
                        await App.Database.AddLog(newlog);
                        await App.Database.AddAnalyticsLog(Alog);
                        SetWorkerList();
                        SetCSEList();
                    }
                    else
                    {
                        await DisplayAlert("Error Worker Entry", "Worker Already Signed In", "Return to Entry");
                        workerpicker.SelectedIndex = -1;
                    }
                }
                else
                {
                    var properworker = currentworkers.Where(w => w.WorkerID == selectedworker & w.InOut == 1);
                    int properworker2 = properworker.Count();
                    if (properworker2 == 1)
                    {
                        var deleteworker = properworker.FirstOrDefault();
                  
                        await App.Database.DeleteLog(deleteworker.EntryID);

                        //worker = await App.Database.GetWorker(selectedworker);

                        //newlog.WorkerID = selectedworker;
                        //newlog.Company = worker.Company;
                        //newlog.FirstName = worker.FirstName;
                        //newlog.LastName = worker.LastName;
                        //newlog.InOut = 1;
                        //newlog.TimeLog = DateTime.Now;
                        //newlog.VesselName = currentvessel;

                        //App.Database.AddLog(newlog);
                        SetWorkerList();
                        SetCSEList();
                    }
                    else
                    {
                        await DisplayAlert("Error Worker Entry", "Worker Already Signed Out", "Return to Entry");
                        workerpicker.SelectedIndex = -1;
                    }
                }
            }
        }
        async void WorkerExitSelected(object sender, EventArgs e)
        {
           // int selectedIndex = workersview.ItemSelected;
           // var selectedworker = workersview.ItemsSource.;
        }
            private async void OnMainNavClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
        }

        private async void OnClearButtonClicked(object sender, EventArgs e)
        {
            await App.Database.ClearLogs();
            SetCSEList();

        }

        //Nav Bar Show/Hide
        private async void ShowNavClicked(object sender, EventArgs e)
        {
            if (Nav)
            {
                Nav = false;
                NavBarGrid.RowDefinitions[1].Height = 0;
                NavBarGrid.RowDefinitions[1].Height = 0;
            }
            else
            {
                Nav = true;
                NavBarGrid.RowDefinitions[1].Height = 15;
                NavBarGrid.RowDefinitions[1].Height = 60;
            }
        }
        private async void NavSwipedUp(object sender, EventArgs e)
        {
            if (!Nav)
            {
                Nav = true;
                NavBarGrid.RowDefinitions[1].Height = 15;
                NavBarGrid.RowDefinitions[1].Height = 60;
            }
        }
        private async void NavSwipedDown(object sender, EventArgs e)
        {
            if (!Nav)
            {
                Nav = false;
                NavBarGrid.RowDefinitions[1].Height = 0;
                NavBarGrid.RowDefinitions[1].Height = 0;
            }
        }

        //Nav Bar Navigations
        private async void OnCSEManagerClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new CSEntryPage(), false);

        }
        private async void OnSiteStatusButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SiteStatusPage(), false);

        }
        private async void OnVesselButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new VesselEntryPage(), false).ConfigureAwait(false);

        }
        private async void OnWorkerButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new WorkerEntryPage(), false).ConfigureAwait(false);

        }
        private async void OnAnalyticsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new AnalyticsPage(), false).ConfigureAwait(false);

        }
    }
}