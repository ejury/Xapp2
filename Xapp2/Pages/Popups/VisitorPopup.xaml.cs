using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;
using Xapp2.Pages.Popups;

namespace Xapp2.Pages.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VisitorPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        private TaskCompletionSource<(bool isAccepted, string temp)> _taskCompletionSource;
        public Task<(bool isAccepted, string temp)> PopupClosedTask => _taskCompletionSource.Task;

        string currentunit;
        string currentvessel;

        public VisitorPopup(string CSEUnit, string CSEVessel)
        {
            InitializeComponent();
            currentunit = CSEUnit;
            currentvessel = CSEVessel;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _taskCompletionSource = new TaskCompletionSource<(bool isAccepted, string temp)>();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _taskCompletionSource.SetResult((true, "temp"));
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {

            //Ensure full name info has been entered
            if (FirstNameText.Text != null & LastNameText.Text != null & CompanyNameText.Text != null)
            {
                string FirstEntry = FirstNameText.Text;
                string LastEntry = LastNameText.Text;
                string CompanyEntry = CompanyNameText.Text;  //Test to see if manual company name was entered

                //Check if User name already exists
                var workerslist = await App.Database.GetWorkers();
                var properworker = workerslist.Where(w => w.FirstName == FirstEntry & w.LastName == LastEntry);
                int properworker2 = properworker.Count();
                if (properworker2 == 0)
                {
                    Worker workers = new Worker();
                    //string newworker = ((Entry)sender).Text;
                    workers.FirstName = FirstEntry;
                    workers.LastName = LastEntry;
                    workers.Activated = 1;
                    workers.SELevel = 6;
                    workers.ReferenceNFC = Globals.ServerName + "_V";
                    workers.Company = CompanyEntry;
                    workers.CreatedTime = DateTime.Now;

                    //Add worker to database
                    int ValidCard = await App.Database.AddWorker(workers); //ID number is added to ReferenceID during database function. Change carries back to original param
                    if (ValidCard == 0)//Card already active flag from dB
                    {
                        await DisplayAlert("Error Visitor Creation", "Card already assigned to a user", "Return to Entry");
                    }
                    if (ValidCard == -1)//Card not valid in dB
                    {
                        await DisplayAlert("Error Visitor Creation", "Invalid Card Used", "Return to Entry");
                    }
                    if (ValidCard == -2)//Name&Company in use
                    {
                        await DisplayAlert("Error Visitor Creation", "Username already created at Company", "Click to Sync Database, Then Retry");
                        await App.Database.GetWorkersAPI();
                    }
                    if (ValidCard > 0)
                    {
                        //Create Visitor Log
                        Newlogentry(workers);
                    }
                }
                else
                {
                    //Create Visitor Log
                    Newlogentry((Worker)properworker.First());
                }
                //Close popup
                await PopupNavigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error Worker Creation", "All fields not entered", "Return to Entry");
            }
                
        }
        public async void Newlogentry(Worker visitor)
        {
            EntryLog newlog = new EntryLog();
            newlog.ReferenceNFC = visitor.ReferenceNFC;
            newlog.TimeLog = DateTime.Now;
            newlog.VesselName = currentvessel;
            newlog.UnitName = currentunit;

            AnalyticsLog Alog = new AnalyticsLog();
            Alog.ReferenceNFC = visitor.ReferenceNFC;
            Alog.InOut = 1;
            Alog.TimeLog = DateTime.Now;
            Alog.VesselName = currentvessel;
            Alog.UnitName = currentunit;

            await App.Database.AddLog(newlog);
            await App.Database.AddAnalyticsLog(Alog);
        }
    }
}