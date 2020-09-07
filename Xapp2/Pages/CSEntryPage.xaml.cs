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
using Syncfusion.XForms.Graphics;
using Rg.Plugins.Popup.Services;
using Xapp2.Pages.Popups;
using Rg.Plugins.Popup.Contracts;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

        public partial class CSEntryPage : ContentPage
    { 
        public string currentunit;
        public string currentvessel;
        public bool ListViewMode;
        bool IsScanning = false;
        List<EntryLog> currentlogs;
        EntryLog newlog = new EntryLog();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SetUnitList();
            ListViewMode = true;
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
        private async void ScanClicked(object sender, EventArgs e)
        {
            if (IsScanning)
            {
                IsScanning = false;
                ScanButton.Text = "Start Scanner";
                ScanButtonColor.Color = Color.FromHex("#51F1F2");
                ScanButtonColor2.Color = Color.LightGray;
                ScanButtonColor3.Color = Color.LightGray;
                PageLayout.BackgroundColor = Color.FromHex("#f0f3f6");
                VisitorButton.IsVisible = false;
                VisitorButton2.IsVisible = false;
            }
            else
            {
                IsScanning = true;
                ScanButton.Text = "Swipe SE Card";
                ScanButtonColor.Color = Color.FromHex("#5bf2df");
                ScanButtonColor2.Color = Color.FromHex("#78e7ff");
                ScanButtonColor3.Color = Color.FromHex("#78e7ff");
                //PageLayout.BackgroundColor = Color.FromHex("#6bb4b6");
                PageLayout.BackgroundColor = Color.FromHex("#226776");
                VisitorButton.IsVisible = true;
                VisitorButton2.IsVisible = true;
            }
        }
        private async void VisitorClicked(object sender, EventArgs e)
        {
            if (currentunit == null | currentvessel == null)
            {
                await DisplayAlert("Visitor Sign-in Error", "Location & Area not selected", "Return to Portal");
            }
            else
            {
                var inputPopup = new VisitorPopup(currentunit, currentvessel);
                await PopupNavigation.Instance.PushAsync(inputPopup, true);
                var ret = await inputPopup.PopupClosedTask;
                SetCSEList();
            }
        }
        private async void VisitorOutClicked(object sender, EventArgs e)
        {
            if (currentunit == null | currentvessel == null)
            {
                await DisplayAlert("Visitor Sign-in Error", "Location & Area not selected", "Return to Portal");
            }
            else
            {
                List<EntryLog> loglistall = await App.Database.GetLogs();
                List<EntryLog> loglist = loglistall.Where(w => w.UnitName == currentunit & w.VesselName == currentvessel & w.ReferenceNFC.StartsWith(Globals.ServerName+"_V")).ToList();
                //////////////////////////////// Need to create list of visitors only

                var workerlistall = await App.Database.GetWorkers();
                workerlistall = workerlistall.Where(w => w.ReferenceNFC.StartsWith(Globals.ServerName + "_V")).ToList();

                //Viewmodel for displaying list
                List<TimeDisplay> timedisplay = new List<TimeDisplay>();
                //List<string> NFClist = new List<string>();
                CultureInfo format = new CultureInfo("en-US");

                //Pulling worker info from NFC tags and adding to UI display
                for (int i = 0; i < loglist.Count; i++)
                {
                    TimeDisplay tempdisplay = new TimeDisplay();
                    Worker tempworker = workerlistall.Where(w => w.ReferenceNFC == loglist[i].ReferenceNFC).FirstOrDefault();

                    tempdisplay.DateLog = loglist[i].TimeLog.ToString("dddd-dd", format);
                    tempdisplay.TimeLog = loglist[i].TimeLog.ToString("h:mm tt", format);
                    tempdisplay.FirstName = tempworker.FirstName;
                    tempdisplay.LastName = tempworker.LastName;
                    tempdisplay.Company = tempworker.Company;
                    tempdisplay.EntryID = loglist[i].EntryID;
                    timedisplay.Add(tempdisplay);
                    //NFClist.Add(tempworker.ReferenceNFC);
                }

                if (timedisplay.Count == 0)
                {
                    await DisplayAlert("Visitor Sign-out Error", "No Visitors at Location", "Return to Portal");
                }
                else
                {
                    var inputPopup = new VisitorOutPopup(timedisplay);
                    await PopupNavigation.Instance.PushAsync(inputPopup, true);
                    var ret = await inputPopup.PopupClosedTask;
                    SetCSEList();
                }
            }
        }
            private async void SetCSEList()
        {
            //Setting appropriate view state
            if (ListViewMode==true)
            {
                LView1.TextColor= Color.White;
                RView1.TextColor = Color.Aqua;
                RView2.TextColor = Color.Aqua;
                workersview.HeightRequest = 0;
                CompanyList.HeightRequest = 400;
                ListSym1.IsVisible = false;
                ListSym2.IsVisible = true;
                ListSym3.IsVisible = true;
                Listname.Text = "Groups";
            }
            else
            {
                LView1.TextColor = Color.Aqua;
                RView1.TextColor = Color.White;
                RView2.TextColor = Color.White;
                workersview.HeightRequest = 400;
                CompanyList.HeightRequest = 0;
                ListSym1.IsVisible = true;
                ListSym2.IsVisible = false;
                ListSym3.IsVisible = false;
                Listname.Text = "Users";
            }

            //Pull local database info to gather CSE entries
            List<EntryLog> loglistall = await App.Database.GetLogs();
            List<EntryLog> loglistU = loglistall.Where(w => w.UnitName == currentunit).ToList();
            List<EntryLog> loglist = loglistU.Where(w => w.VesselName == currentvessel).ToList();
            currentlogs = loglist.ToList();
            var workerlistall = await App.Database.GetWorkers();

            //Viewmodel for displaying list
            List<TimeDisplay> timedisplay = new List<TimeDisplay>();
            CultureInfo format = new CultureInfo("en-US");

            //Pulling worker info from NFC tags and adding to UI display
            for (int i = 0; i < loglist.Count; i++)
            {
                TimeDisplay tempdisplay = new TimeDisplay();
                Worker tempworker = workerlistall.Where(w => w.ReferenceNFC == loglist[i].ReferenceNFC).FirstOrDefault();

                tempdisplay.DateLog = loglist[i].TimeLog.ToString("dddd-dd", format);
                tempdisplay.TimeLog = loglist[i].TimeLog.ToString("h:mm tt", format);
                tempdisplay.FirstName = tempworker.FirstName;
                tempdisplay.LastName = tempworker.LastName;
                tempdisplay.Company = tempworker.Company;
                timedisplay.Add(tempdisplay);
            }
            workersview.ItemsSource = timedisplay;

            //Summarizing company count data
            var companylong = timedisplay.Select(c => c.Company).ToList();
            var company = companylong.Distinct().ToList();
            int[] companycount = new int[company.Count];
            List<CompanyList> companylist = new List<CompanyList>();

            //Only go through counters if data exists
            if (loglist.Any() == true)
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
        private void ListViewChange(object sender, EventArgs e)
        {
            if (ListViewMode==true) 
            { ListViewMode = false; }
            else 
            { ListViewMode = true; }
            SetCSEList();
        }
        public CSEntryPage()
        {
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
                //workerpicker.ItemsSource = null;
                //workerpicker.Title = "Select Vessel";

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
                //workerpicker.Title = "Select Worker";
                //SetWorkerList();
                SetCSEList();

                Globals.init = true; Globals.unit = currentunit; Globals.vessel = currentvessel; //Establishing global item selection
            }
        }
        async void OnWorkerPickerChanged(object sender, EventArgs e)
        {
            //Check if worker card exists and is activited
            string NFCSwipe = SwipeEntry.Text;
            var workerlistall = await App.Database.GetWorkers();
            var properworker = workerlistall.Where(w => w.ReferenceNFC == NFCSwipe & w.Activated == 1);
            int properworker2 = properworker.Count();

            

            //If activated worker not found
            if (properworker2 == 0)
            {
                //Force update from database and recheck
                workerlistall = await App.Database.GetWorkersAPI();
                properworker = workerlistall.Where(w => w.ReferenceNFC == NFCSwipe & w.Activated == 1);
                properworker2 = properworker.Count();

                //If database user is not active/exist return error
                if (properworker2 == 0)
                {
                    await DisplayAlert("Error Worker Entry", "Worker Does Not Exist", "Return to Entry");
                    return;
                }
            }

            //Checking if user was already signed in
            List<EntryLog> Loglistall = await App.Database.GetLogs();
            List<EntryLog> LoglistU = Loglistall.Where(w => w.UnitName == currentunit).ToList();
            List<EntryLog> Loglist = LoglistU.Where(w => w.VesselName == currentvessel & w.ReferenceNFC == NFCSwipe).ToList();
            int Loglist2 = Loglist.Count();

            if (Loglist2 == 0) //If Not signed in, create new log
            {
                Worker worker = new Worker();
                worker = properworker.FirstOrDefault();

/*                newlog.WorkerID = worker.WorkerID; //Removed due to NFC tie to worker. Redundant
                newlog.Company = worker.Company;
                newlog.FirstName = worker.FirstName;
                newlog.LastName = worker.LastName;*/
                newlog.ReferenceNFC = worker.ReferenceNFC;
                newlog.TimeLog = DateTime.Now;
                newlog.VesselName = currentvessel;
                newlog.UnitName = currentunit;

                AnalyticsLog Alog = new AnalyticsLog();
                Alog.ReferenceNFC = worker.ReferenceNFC;
                Alog.InOut = 1;
                Alog.TimeLog = DateTime.Now;
                Alog.VesselName = currentvessel;
                Alog.UnitName = currentunit;
                      
                await App.Database.AddLog(newlog);
                await App.Database.AddAnalyticsLog(Alog);

                SetCSEList();

            }
            else //If signed in, delete log
            {
                EntryLog templog = new EntryLog();
                templog = Loglist[0];

                await App.Database.DeleteLog(templog.EntryID);

                    SetCSEList();
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