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
using Plugin.NFC;
using Xamarin.Essentials;

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

        //Main NFC Scanner button selected - modify GUI veriables
        private async void ScanClicked(object sender, EventArgs e)
        {
            //Do not allow unauthorized user to initiate scanning
            if (Globals.SELevel > 3)
            {
                await DisplayAlert("Unauthorized", "User does not have authorization for scanning functionality. Contact your administrator", "Return to Portal");
                return;
            }
            //Provide warning prior to offline scanning 
            if (!IsScanning & (Globals.OfflineMode == true | Connectivity.NetworkAccess != NetworkAccess.Internet))
            {
                bool answer = await DisplayAlert("Warning", "You are about to handle user sign-in in offline mode. All actions will not be updated in central database unless sync is completed. Continue?", "Yes","No");
                if (answer)
                {
                    // Insert code here for showing sync button on GUI
                    Globals.OfflineMode = true;
                    SyncButton.IsVisible = true;
                    SyncText.IsVisible = true;
                }
                else
                { return; }
            }
            if (currentunit == null | currentvessel == null)
            {
                await DisplayAlert("Visitor Sign-in Error", "Location & Area not selected", "Return to Portal");
                return;
            }




                //Update GUI and initiate NFC scanning tool
                if (IsScanning)
            {

                UnsubscribeEvents();
                AIndicator.IsRunning = false;
                AIndicator.IsEnabled = false;

                await Task.WhenAll(
                   VisitorButton.FadeTo(0, 500), VisitorButton2.FadeTo(0, 500),
                    NFCimage2.TranslateTo(-400, 0, 1000), NFCimage2.FadeTo(0, 750),
                    NFCtail2.TranslateTo(-400, 0, 1000), NFCtail2.FadeTo(0, 750)
                );
                await Task.WhenAll(
                    unitpicker.FadeTo(1, 500), Xunitpicker.FadeTo(1, 500), vesselpicker.FadeTo(1, 500), Xvesselpicker.FadeTo(1, 500),
                     NFCimage.TranslateTo(0, 0, 1), NFCimage.FadeTo(1, 750),
                     NFCtail.TranslateTo(0, 0, 1), NFCtail.FadeTo(1, 750)
);
                //Update GUI
                IsScanning = false;
                MidLabel.Text = "Occupied List";
                ScanButton.IsVisible = true; ScanButton2.IsVisible = false;

                NavBarGrid.HeightRequest = 50;
/*                VisitorButton.IsVisible = false; 
                VisitorButton2.IsVisible = false; */
            }
            else
            {
#if DEBUG //Verify NFC for release code

#else
                //Verify NFC scanner is active
                if (CrossNFC.IsSupported)
                {
                    if (!CrossNFC.Current.IsAvailable)
                    {
                        await DisplayAlert("NFC is not available", "Your device is not equiped with NFC capability for use of tool", "return to page");
                        return;
                    }
                    NfcIsEnabled = CrossNFC.Current.IsEnabled;
                    if (!NfcIsEnabled)
                    {
                        await DisplayAlert("NFC is not active", "Please enable NFC in your device's options to use tool", "return to page");
                        return;
                    }
                    
                    SubscribeEvents();
                    StartListeningIfNotiOS();
                }
#endif
                await Task.WhenAll(
                unitpicker.FadeTo(0, 500), Xunitpicker.FadeTo(0, 500), vesselpicker.FadeTo(0, 500), Xvesselpicker.FadeTo(0, 500),

                NFCimage.TranslateTo(400, 0, 1000),NFCimage.FadeTo(0,750),
                NFCtail.TranslateTo(400,0,1000), NFCtail.FadeTo(0, 750)
                );

                await Task.WhenAll(
                   VisitorButton.FadeTo(1, 500), VisitorButton2.FadeTo(1, 500),

                     NFCimage2.TranslateTo(0,0, 1), NFCimage2.FadeTo(1, 750),
                     NFCtail2.TranslateTo(0,0, 1), NFCtail2.FadeTo(1, 750)
                );
                AIndicator.IsRunning = true;
                AIndicator.IsEnabled = true;
                IsScanning = true;
                MidLabel.Text = currentvessel + ":";

                ScanButton.IsVisible = false; ScanButton2.IsVisible = true;

                NavBarGrid.HeightRequest = 0;
                VisitorButton.IsEnabled = true; 
                VisitorButton2.IsEnabled = true; 
            }
        }

        private async void SyncClicked(object sender, EventArgs e)
        {
            //Ensure user wants to continue in offline mode
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("Error", "You have lost internet connection. To sync data with server, please connect to wifi or cellular data", "Close");
                return;
            }

            //Obtain new JWT token prior to sync
            Worker tempWorker = new Worker(); //Create temp WorkerID for Web API
            tempWorker.ReferenceNFC = Globals.NFCsignin;

            //Request JWT from Web API
            string IsValid = await APIServer.SEClient(tempWorker);

            //Check for API Error
            if (IsValid == "Card Not Active" | IsValid == "Unable to Sign In" | IsValid == "Invalid Card Used" | IsValid == "Server Error" | IsValid == "Server Connection Error")
            {
                DisplayAlert(IsValid, "Unable to sync data, please retry sign-in and repeat", "Return to login screen");
                return;
            }
            
            

            //Sync will only use analytics logs and then refresh entrylogs with Db. Server side code to manage sync comparison
            var analyticsall = await App.Database.GetAnalytics();
            var analyticslist = analyticsall.Where(c => c.EntryID == -1).ToList();

            //API Sync request
            int isSuccess =  await App.Database.SyncLogs(analyticslist);

            if (isSuccess == 1)
            {
                //Check if internet connection is now in place
                SyncButton.IsVisible = false;
                SyncText.IsVisible = false;
                Globals.OfflineMode = false; 
            }
        }
        //Add visitor selected
        private async void VisitorClicked(object sender, EventArgs e)
        {
            if (currentunit == null | currentvessel == null)
            {
                await DisplayAlert("Visitor Sign-in Error", "Location & Area not selected", "Return to Portal");
                return;
            }

            var inputPopup = new VisitorPopup(currentunit, currentvessel);
            await PopupNavigation.Instance.PushAsync(inputPopup, true);
            var ret = await inputPopup.PopupClosedTask;
            SetCSEList();
            
        }
        //Remove visitor selected
        private async void VisitorOutClicked(object sender, EventArgs e)
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
                    tempdisplay.TimeLog = loglist[i].TimeLog.ToString("MM/dd h:mm tt", format);
                    tempdisplay.FirstName = tempworker.FirstName.FirstOrDefault() +".";
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
        //Set main display of active users in location
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
                tempdisplay.TimeLog = loglist[i].TimeLog.ToString("MM/dd h:mm tt", format);
                tempdisplay.FirstName = tempworker.FirstName.FirstOrDefault() + ".";
                tempdisplay.LastName = tempworker.LastName;
                tempdisplay.Company = tempworker.Company;
                tempdisplay.ReferenceNFC = tempworker.ReferenceNFC;
                tempdisplay.EntryID = loglist[i].EntryID;
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

        //Update selected Area upon user selection and update location list
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
        //Update selected Location upon user selection and update GUI display area
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

        //Temporary sign in for manual NFC entry
        async void tempNFCcompleted(object sender, EventArgs e)
        {
            //Check if worker card exists and is activited
            string NFCSwipe = TempSECardEntry.Text;
            WorkerInOut(NFCSwipe);
        }
        //Sign In or Out user based on new swipe
        async void WorkerInOut(string NFCSwipe)
        {
            //Check if location has been properly selected
            if (currentunit == null | currentvessel == null)
            {
                await DisplayAlert("Error Worker Entry", "Location not selected", "Return to Entry");
                return;
            }
            //Check if internet access was lost (not yet warned to sync) since scanning initiated
            if (!SyncButton.IsVisible & Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                bool answer = await DisplayAlert("Warning", "You have lost internet connection. All actions will not be updated in central database unless sync is completed. Continue?", "Yes", "No");
                if (answer)
                {
                    // Insert code here for showing sync button on GUI
                    SyncButton.IsVisible = true;
                    SyncText.IsVisible = true;
                    Globals.OfflineMode = true;
                }
                else
                { return; }
            }

            //Verify signin vs signout while waiting for internet sync

            //Check if worker card exists and is activited
            var workerlistall = await App.Database.GetWorkers();
            var properworker = workerlistall.Where(w => w.ReferenceNFC == NFCSwipe & w.Activated == 1);
            int properworker2 = properworker.Count();

            //If activated worker not found
            if (properworker2 == 0)
            {
                if (!SyncButton.IsVisible)
                {
                    //Force update from database and recheck
                    workerlistall = await App.Database.GetWorkersAPI();
                    properworker = workerlistall.Where(w => w.ReferenceNFC == NFCSwipe & w.Activated == 1);
                    properworker2 = properworker.Count();
                }
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

                await CreateLog(worker.ReferenceNFC, 1, 0);

                SetCSEList();
            }
            else //If signed in, delete log
            {
                EntryLog templog = new EntryLog();
                templog = Loglist[0];

                await CreateLog(templog.ReferenceNFC, -1, templog.EntryID);

                SetCSEList();
            }
        }
        async void WorkerExitSelected(object sender, EventArgs e)
        {
            var LineSelectedvar = workersview.SelectedItem;
            TimeDisplay LineSelected = (TimeDisplay)LineSelectedvar;

            if (LineSelected != null) //Don't display options if deselecting was trigger
            {
                string answer = await DisplayActionSheet("User Selection Options", "Cancel", null, "Manual Sign-Out");

                if (answer == "Manual Sign-Out") //Sign out Logged User
                {
                    await CreateLog(LineSelected.ReferenceNFC, -1, LineSelected.EntryID);
                    SetCSEList();
                }
                workersview.SelectedItem = null;
            }
        }

        async Task CreateLog(string NFCtag, int Inout, int Lognum) //Set Lognum to 0 if entry
        {
            EntryLog Elog = new EntryLog();
            newlog.ReferenceNFC = NFCtag;
            newlog.TimeLog = DateTime.Now;
            newlog.VesselName = currentvessel;
            newlog.UnitName = currentunit;

            AnalyticsLog Alog = new AnalyticsLog();
            Alog.ReferenceNFC = NFCtag;
            Alog.InOut = Inout;
            Alog.TimeLog = DateTime.Now;
            Alog.VesselName = currentvessel;
            Alog.UnitName = currentunit;

            if (Inout == -1)
            {
                await App.Database.DeleteLog(Lognum); 
                await App.Database.AddAnalyticsLog(Alog);
            }
            

            if (Inout == 1)
            {
                await App.Database.AddLog(newlog);
                await App.Database.AddAnalyticsLog(Alog);
            }
            
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
            await Task.WhenAll(
                CSEButton.FadeTo(1.0, 500), StatusButton.FadeTo(0.5, 500), HeirarchyButton.FadeTo(0.5, 500), WorkerButton.FadeTo(0.5, 500), AnalyticsButton.FadeTo(0.5, 500),
                CSEButton.ScaleTo(1.15,500));
            await Navigation.PushModalAsync(new CSEntryPage(), false);
        }
        private async void OnSiteStatusButtonClicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                CSEButton.FadeTo(0.5, 500), StatusButton.FadeTo(1.0, 500), HeirarchyButton.FadeTo(0.5, 500), WorkerButton.FadeTo(0.5, 500), AnalyticsButton.FadeTo(0.5, 500),
                StatusButton.ScaleTo(1.15, 500));
            await Navigation.PushModalAsync(new SiteStatusPage(), false);
        }
        private async void OnVesselButtonClicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                CSEButton.FadeTo(0.5, 500), StatusButton.FadeTo(0.5, 500), HeirarchyButton.FadeTo(1.0, 500), WorkerButton.FadeTo(0.5, 500), AnalyticsButton.FadeTo(0.5, 500),
                HeirarchyButton.ScaleTo(1.15, 500));
            await Navigation.PushModalAsync(new VesselEntryPage(), false).ConfigureAwait(false);
        }
        private async void OnWorkerButtonClicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                CSEButton.FadeTo(0.5, 500), StatusButton.FadeTo(0.5, 500), HeirarchyButton.FadeTo(0.5, 500), WorkerButton.FadeTo(1.0, 500), AnalyticsButton.FadeTo(0.5, 500),
                WorkerButton.ScaleTo(1.15, 500));
            await Navigation.PushModalAsync(new WorkerEntryPage(), false).ConfigureAwait(false);
        }
        private async void OnAnalyticsButtonClicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                CSEButton.FadeTo(0.5, 500), StatusButton.FadeTo(0.5, 500), HeirarchyButton.FadeTo(0.5, 500), WorkerButton.FadeTo(0.5, 500), AnalyticsButton.FadeTo(1.0, 500),
                AnalyticsButton.ScaleTo(1.15, 500));
            await Navigation.PushModalAsync(new AnalyticsPage(), false).ConfigureAwait(false);
        }


        ////////////////////////// NFC CODE //////////////////////////////////

        //NFC Variables
        public const string ALERT_TITLE = "NFC";
        public const string MIME_TYPE = "application/com.companyname.Jurisoft";

        NFCNdefTypeFormat _type;
        bool _makeReadOnly = false;
        bool _eventsAlreadySubscribed = false;
        bool ChkReadOnly = false;

        private bool _nfcIsEnabled;
        public bool NfcIsEnabled
        {
            get => _nfcIsEnabled;
            set
            {
                _nfcIsEnabled = value;
                OnPropertyChanged(nameof(NfcIsEnabled));
                OnPropertyChanged(nameof(NfcIsDisabled));
            }
        }
        public bool NfcIsDisabled => !NfcIsEnabled;

        protected override bool OnBackButtonPressed()
        {
            UnsubscribeEvents();
            CrossNFC.Current.StopListening();
            return base.OnBackButtonPressed();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UnsubscribeEvents(); //Ensure NFC queries are reset on leaving page

        }

        /// <summary>
        /// Subscribe to the NFC events
        /// </summary>
        void SubscribeEvents()
        {
            if (_eventsAlreadySubscribed)
                return;

            _eventsAlreadySubscribed = true;

            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
           // CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
           // CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;

            if (Device.RuntimePlatform == Device.iOS)
                CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
        }

        /// <summary>
        /// Unsubscribe from the NFC events
        /// </summary>
        void UnsubscribeEvents()
        {
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
           // CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
           // CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
            CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

            if (Device.RuntimePlatform == Device.iOS)
                CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;
        }

        /// <summary>
        /// Event raised when NFC Status has changed
        /// </summary>
        /// <param name="isEnabled">NFC status</param>
        async void Current_OnNfcStatusChanged(bool isEnabled)
        {
            NfcIsEnabled = isEnabled;
            await ShowAlert($"NFC has been {(isEnabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Event raised when a NDEF message is received
        /// </summary>
        /// <param name="tagInfo">Received <see cref="ITagInfo"/></param>
        async void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            if (tagInfo == null)
            {
                await ShowAlert("No tag found");
                return;
            }

            // Customized serial number
            var identifier = tagInfo.Identifier;
            var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
            var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"Tag [{serialNumber}]" : "Tag Info";

            if (!tagInfo.IsSupported)
            {
                await ShowAlert("Unsupported tag (app)", title);
            }
            else if (tagInfo.IsEmpty)
            {
                await ShowAlert("Empty tag", title);
            }
            else
            {
                var first = tagInfo.Records[0];
                
                try
                {
                    WorkerInOut(first.Message);
                    TempSECardEntry.Text = first.Message;
                }
                catch
                {
                    TempSECardEntry.Text = "Error";
                }
                
                
            }
        }

        /// <summary>
        /// Event raised when user cancelled NFC session on iOS 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) => Debug("User has cancelled NFC Session");

        /// Write a debug message in the debug console
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        void Debug(string message) => System.Diagnostics.Debug.WriteLine(message);

        /// <summary>
        /// Display an alert
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="title">Alert title</param>
        /// <returns>The task to be performed</returns>
        Task ShowAlert(string message, string title = null) => DisplayAlert(string.IsNullOrWhiteSpace(title) ? ALERT_TITLE : title, message, "Cancel");

        /// <summary>
        /// Task to start listening for NFC tags if the user's device platform is not iOS
        /// </summary>
        /// <returns>The task to be performed</returns>
        async Task StartListeningIfNotiOS()
        {
            if (Device.RuntimePlatform == Device.iOS)
                return;
            await BeginListening();
        }

        /// <summary>
        /// Task to safely start listening for NFC Tags
        /// </summary>
        /// <returns>The task to be performed</returns>
        async Task BeginListening()
        {
            try
            {
                CrossNFC.Current.StartListening();
                AIndicator.IsRunning = true;
            }
            catch (Exception ex)
            {
                await ShowAlert(ex.Message);
            }
        }
    }
}