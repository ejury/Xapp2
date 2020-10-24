using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;
using Xapp2.Models.ViewModel;
using Plugin.NFC;
using Xamarin.Essentials;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkerEntryPage : ContentPage
    {
        WorkerDoughnutView doughnutview = new WorkerDoughnutView();
        int currentselect;
        bool Nav;
        int ListViewMode = 2; //Captured data view mode
        public string currentcompany;
        Task<List<Worker>> _fetchingWorkers;
        Worker workers = new Worker();

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var workerlist = await App.Database.GetWorkers();
            List<string> companylistlong = workerlist.Select(c => c.Company).ToList();
            var companylist = companylistlong.GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .ToList();

            if (companylist.Count > 0 & workerlist.Count > 0)
            {
                //Order Company list from largest to smallest (ensures pieview 'others' does not mix up selected index variables
                /*                List<int> Ccounts = new List<int>();
                                for (int t= 0; t < companylist.Count; t++)
                                {
                                    Ccounts[t] = companylistlong.Where(c => c.Equals(companylist[t])).Count();
                                }*/


                companypicker.ItemsSource = companylist;

                //Set Initial Unit Selection
                currentselect = 0;
                doughnutview.doughnutselect = currentselect;
                companypicker.SelectedIndex = currentselect;
                currentcompany = companylist[currentselect];
                SetWorkerList();
            }
        }

        private async void SetWorkerList()
        {
            var workerlist = await App.Database.GetWorkers();
            workerlist.RemoveAll(s => s.SELevel == 6); //Remove visitors from list
            workerlist.RemoveAll(s => s.Activated == 0); //Remove non activated cards from list
            List<string> companylistlong = workerlist.Select(c => c.Company).ToList();
            var companylist = companylistlong.GroupBy(x => x)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .ToList();

            FilterWorkerList(workerlist);

            //Generating Pie Chart
            int t = companylist.Count;
            WorkerDoughnutModel tempdoughnut = new WorkerDoughnutModel();
            string tempcompany;

            doughnutview.doughnutmodel.Clear();   //Clear old entries
            doughnutview.doughnutlabels.Clear();  //Clear old entries
            for (int i = 0; i < t; i++)
            {
                tempdoughnut.Companyname = companylist[i];
                tempdoughnut.Workercount = workerlist.Where(w => w.Company == companylist[i]).Count();

                //Updating Pie Chart
                doughnutview.doughnutmodel.Add(tempdoughnut);
                doughnutview.doughnutlabels.Add(companylist[i]);
            }
            doughnut.ExplodeIndex = doughnutview.doughnutselect;
        }
        private async void FilterWorkerList(List<Worker> workerlist)
        {
            //Update worker list for the current selected company

            var workerlist2 = workerlist.Where(w => w.Company == currentcompany);

/*            //Abreviate first names to one Char if on shortened viewmode
            if (ListViewMode == 1)
            {
                for (int i = 0; i < workerlist2.Count(); i++)
                {
                    workerlist2.ElementAt(i).FirstName = workerlist2.ElementAt(i).FirstName.FirstOrDefault().ToString() + ".";
                }
            }*/
            workersview.ItemsSource = workerlist2;
            workersview2.ItemsSource = workerlist2;
            companylabelname.Text = currentcompany;

        }
        public WorkerEntryPage()
        {
            Nav = true;
            InitializeComponent();
            BindingContext = doughnutview;
        }

        private void ListViewChange(object sender, EventArgs e)
        {
            workersview.IsVisible = false;
            workersview2.IsVisible = false;
            //Move listview mode through 3 states - 1=both visible, 2=pie visible, 3=list visible
            if (ListViewMode < 3)
            { ListViewMode++; }
            else
            { ListViewMode = 1; }

            //Setting appropriate view state
            if (ListViewMode == 1)
            {
                LView1.TextColor = Color.Aqua;
                RView1.TextColor = Color.Aqua;
                RView2.TextColor = Color.Aqua;

                PieFrameLegend.IsVisible = false;
                LDisplay.Width = GridLength.Star;
                RDisplay.Width = GridLength.Star;
                XAMLdatelist.Width = 0;
                workersview2.IsVisible = true;
            }
            if (ListViewMode == 2)
            {
                LView1.TextColor = Color.Aqua;
                RView1.TextColor = Color.White;
                RView2.TextColor = Color.White;

                PieFrameLegend.IsVisible = true;
                LDisplay.Width = GridLength.Star;
                RDisplay.Width = 0;
                workersview2.IsVisible = true;
            }
            if (ListViewMode == 3)
            {
                LView1.TextColor = Color.White;
                RView1.TextColor = Color.Aqua;
                RView2.TextColor = Color.Aqua;

                LDisplay.Width = 0;
                RDisplay.Width = GridLength.Star;
                workersview.IsVisible = true;
                XAMLdatelist.Width = GridLength.Star;
            }

            SetWorkerList();
        }

        void PieChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "ExplodeIndex") //Only trigger update for new chart selection (Optimization)
            {
                OnPickerSelectedIndexChanged(sender,e);
            }
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            //Error handler due to events during page changes. Verifies both pickers can be accessed and skips updates if models not loaded
            int selectedIndex = -1;
            try
            {
                if (companypicker.SelectedIndex != -1)
                { selectedIndex = companypicker.SelectedIndex; }
                if (doughnut.ExplodeIndex != -1)
                { selectedIndex = doughnut.ExplodeIndex; }
            }
            catch (Exception e1)
            {  
                selectedIndex = -1; 
            }


            int changed = 0; //test if company selection state has changed to avoid circular loop

            if (selectedIndex != -1)
            {

                //Updating company selection state variables
                if (doughnut.ExplodeIndex != currentselect & doughnut.ExplodeIndex != -1) //checks if chart was updated
                {
                    currentselect = doughnut.ExplodeIndex;
                    doughnutview.doughnutselect = currentselect;
                    companypicker.SelectedIndex = currentselect;
                    changed = 1;
                }
                if (companypicker.SelectedIndex != currentselect & companypicker.SelectedIndex != -1 & changed==0) //checks if picker was updated
                {
                    currentselect = companypicker.SelectedIndex;
                    doughnutview.doughnutselect = currentselect;
                    changed = 1;
                }

                //Updating view models if state has changed
                if (changed == 1)
                {
                    //currentcompany = (string)picker.ItemsSource[selectedIndex];
                    currentcompany = (string)companypicker.ItemsSource[currentselect];
                    workers.Company = currentcompany;
                    CompanyNameText.Text = string.Empty;
                    SetWorkerList();
                }
            }

        }

        //Temporary sign in for manual NFC entry
        async void tempNFCcompleted(object sender, EventArgs e)
        {
            //Check if worker card exists and is activited
            string NFCSwipe = TempSECardEntry.Text;
            WorkerInOut(NFCSwipe);
        }

        async void NewWorkerClicked(object sender, EventArgs e)
        {
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
                AIndicator.IsRunning = true;

            }
        }

        async Task WorkerInOut(string NFCSwipe)
        {
#if DEBUG //Verify internet connection for any new Db items for release code

#else
         {
                        if (Connectivity.NetworkAccess != NetworkAccess.Internet )
                            {
                                await DisplayAlert("Error Area Creation", "Cannot add new Area without internet connection", "Obtain connection and retry");
                                return;
                            }
                        if ( Globals.OfflineMode == true)
                            {
                                await DisplayAlert("Error Area Creation", "Currently signed in under Offline Mode. Logout and sign in with internet connection", "Return to Heirarchy Page");
                                return;
                            }
            }
#endif

            //Ensure full name info has been entered
            if (FirstNameText.Text != null & LastNameText.Text != null)
            {
                string FirstEntry = FirstNameText.Text;
                string LastEntry = LastNameText.Text;

                string ManualCompanyEntry = CompanyNameText.Text;  //Test to see if manual company name was entered
                if (ManualCompanyEntry != null)
                {
                    if (ManualCompanyEntry.Length > 0)
                    {
                        currentcompany = ManualCompanyEntry;
                        companypicker.ItemsSource.Add(currentcompany);
                    }
                }

                //Check if User name already exists
                var workerslist = await App.Database.GetWorkers();
                var properworker = workerslist.Where(w => w.FirstName == FirstEntry & w.LastName == LastEntry);
                int properworker2 = properworker.Count();
                if (properworker2 == 0)
                {
                    //string newworker = ((Entry)sender).Text;
                    workers.FirstName = FirstEntry;
                    workers.LastName = LastEntry;
                    workers.Activated = 1;
                    Globals.NFCtempcount++;
                    //workers.ReferenceNFC = Globals.NFCtempcount.ToString();
                    workers.ReferenceNFC = NFCSwipe;
                    workers.Company = currentcompany;
                    workers.CreatedTime = DateTime.Now;

                    //Add worker to database
                    int ValidCard = await App.Database.AddWorker(workers);
                    if (ValidCard == 0)//Card already active flag from dB
                    {
                        await DisplayAlert("Error Worker Creation", "Card already assigned to a user", "Return to Entry");
                    }
                    if (ValidCard == -1)//Card not valid in dB
                    {
                        await DisplayAlert("Error Worker Creation", "Invalid Card Used", "Return to Entry");
                    }
                    if (ValidCard == -2)//Name&Company in use
                    {
                        await DisplayAlert("Error Worker Creation", "Username already created at Company", "Click to Sync Database, Then Retry");
                        await App.Database.GetWorkersAPI();
                    }

                    FirstNameText.Text = string.Empty;
                    LastNameText.Text = string.Empty;
                    CompanyNameText.Text = string.Empty;

                    SetWorkerList();
                }
                else
                {
                    await DisplayAlert("Error Worker Creation", "Worker Name Alreadly In Use", "Return to Entry");
                }
            }
            else
            {
                await DisplayAlert("Worker Entry Error", "Full name has not been entered", "Return to Entry");
            }
        }

        async void WorkerSelected(object sender, EventArgs e)
        {
        //Grabbing selected Vessel Object

            var LineSelectedx = workersview.SelectedItem;
            try
            {
                int LineSelectedi = (int)workersview.SelectedItem;
                if (LineSelectedi == -1)
                { return; }
            }
            catch (Exception ex) 
            {
                var temp = ex;
            }

            if (LineSelectedx == null) //Don't display options if deselecting was trigger
            {
                return;
            }
            Worker LineSelected = (Worker)LineSelectedx;

            string answer = await DisplayActionSheet("User Selection Options", "Cancel", null, "Update Worker - TBD", "Deactivate SE Badge");

            if (answer == "Deactivate SE Badge")
            {
                bool answer2 = await DisplayAlert("Confirm Deactivation?", "User will no longer be able to sign in/out of locations until a new badge is assigned", "Yes", "No");
                if (answer2)
                {
                    //Delete selected worker and reset displayed list
                    await App.Database.DeleteWorker(LineSelected.WorkerID, LineSelected.ReferenceNFC);
                    SetWorkerList();
                }
            }
            workersview.SelectedItem = -1;
        }
        private async void OnMainNavClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
        }

        //Nav Bar Navigations
        private async void OnCSEManagerClicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                CSEButton.FadeTo(1.0, 500), StatusButton.FadeTo(0.5, 500), HeirarchyButton.FadeTo(0.5, 500), WorkerButton.FadeTo(0.5, 500), AnalyticsButton.FadeTo(0.5, 500),
                CSEButton.ScaleTo(1.15, 500));
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
                await WorkerInOut(first.Message);
                TempSECardEntry.Text = $"{first.Message}";

                //Stop waiting for NFC card
                UnsubscribeEvents();
                AIndicator.IsRunning = false;
            }
        }

        /// <summary>
        /// Event raised when user cancelled NFC session on iOS 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) => Debug("User has cancelled NFC Session");

        /*        /// <summary>
                /// Event raised when data has been published on the tag
                /// </summary>
                /// <param name="tagInfo">Published <see cref="ITagInfo"/></param>
                async void Current_OnMessagePublished(ITagInfo tagInfo)
                {
                    try
                    {
                        ChkReadOnly = false;
                        CrossNFC.Current.StopPublishing();
                        if (tagInfo.IsEmpty)
                            await ShowAlert("Formatting tag operation successful");
                        else
                            await ShowAlert("Writing tag operation successful");
                    }
                    catch (Exception ex)
                    {
                        await ShowAlert(ex.Message);
                    }
                }*/

        /// <summary>
        /// Event raised when a NFC Tag is discovered
        /// </summary>
        /// <param name="tagInfo"><see cref="ITagInfo"/> to be published</param>
        /// <param name="format">Format the tag</param>
        /*        async void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
                {
                    if (!CrossNFC.Current.IsWritingTagSupported)
                    {
                        await ShowAlert("Writing tag is not supported on this device");
                        return;
                    }

                    try
                    {
                        NFCNdefRecord record = null;
                        switch (_type)
                        {
                            case NFCNdefTypeFormat.WellKnown:
                                record = new NFCNdefRecord
                                {
                                    TypeFormat = NFCNdefTypeFormat.WellKnown,
                                    MimeType = MIME_TYPE,
                                    Payload = NFCUtils.EncodeToByteArray("Plugin.NFC is awesome!")
                                };
                                break;
                            case NFCNdefTypeFormat.Uri:
                                record = new NFCNdefRecord
                                {
                                    TypeFormat = NFCNdefTypeFormat.Uri,
                                    Payload = NFCUtils.EncodeToByteArray("https://github.com/franckbour/Plugin.NFC")
                                };
                                break;
                            case NFCNdefTypeFormat.Mime:
                                record = new NFCNdefRecord
                                {
                                    TypeFormat = NFCNdefTypeFormat.Mime,
                                    MimeType = MIME_TYPE,
                                    Payload = NFCUtils.EncodeToByteArray(entryfield.Text)
                                };
                                break;
                            default:
                                break;
                        }

                        if (!format && record == null)
                            throw new Exception("Record can't be null.");

                        tagInfo.Records = new[] { record };

                        if (format)
                            CrossNFC.Current.ClearMessage(tagInfo);
                        else
                        {
                            CrossNFC.Current.PublishMessage(tagInfo, _makeReadOnly);
                        }
                    }
                    catch (Exception ex)
                    {
                        await ShowAlert(ex.Message);
                    }
                }
        */

        /// <summary>
        /// Returns the tag information from NDEF record
        /// </summary>
        /// <param name="record"><see cref="NFCNdefRecord"/></param>
        /// <returns>The tag information</returns>
        /*        string GetMessage(NFCNdefRecord record)
                {
                    var message = $"Message: {record.Message}";
                    message += Environment.NewLine;
                    message += $"RawMessage: {Encoding.UTF8.GetString(record.Payload)}";
                    message += Environment.NewLine;
                    message += $"Type: {record.TypeFormat.ToString()}";

                    if (!string.IsNullOrWhiteSpace(record.MimeType))
                    {
                        message += Environment.NewLine;
                        message += $"MimeType: {record.MimeType}";
                    }

                    return message;
                }
        */
        /// <summary>
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