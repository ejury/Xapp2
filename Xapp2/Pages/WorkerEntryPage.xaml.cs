using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkerEntryPage : ContentPage
    {
        bool Nav;
        public string currentcompany;
        Task<List<Worker>> _fetchingWorkers;
        Worker workers = new Worker();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            SetWorkerList();
        }

        private async void SetWorkerList()
        {
            var workerlist = await App.Database.GetWorkers();
            List<string> companylistlong = workerlist.Select(c => c.Company).ToList();
            List<string> companylist = companylistlong.Distinct().ToList();

            picker.ItemsSource = companylist;
            picker.SelectedIndex = 0;
            // picker.SelectedItem = currentcompany;

            FilterWorkerList();
        }
        private async void FilterWorkerList()
        {
            var workerlist = await App.Database.GetWorkers();
            var workerlist2 = workerlist.Where(w => w.Company == currentcompany);
            workersview.ItemsSource = workerlist2;

        }
        public WorkerEntryPage()
        {
            Nav = true;
            InitializeComponent();
        }

        async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                currentcompany = (string)picker.ItemsSource[selectedIndex];
                workers.Company = currentcompany;
                CompanyNameText.Text = string.Empty;
                FilterWorkerList();
            }

        }
        async void OnManualCompanyChange(object sender, EventArgs e)
        {
            string ManualCompanyEntry = CompanyNameText.Text;
            if (ManualCompanyEntry.Length > 0)
            {
                picker.SelectedItem = string.Empty;
            }
        }
        async void NewWorkerClicked(object sender, EventArgs e)
        {
            
            //Ensure full name info has been entered
            if (FirstNameText.Text != null & LastNameText.Text != null)
            {
                string FirstEntry = FirstNameText.Text;
                string LastEntry = LastNameText.Text;

                string ManualCompanyEntry = CompanyNameText.Text;
                if (ManualCompanyEntry.Length > 0)
                {
                    currentcompany = ManualCompanyEntry;
                }

                var workerslist = await App.Database.GetWorkers();
                var properworker = workerslist.Where(w => w.FirstName == FirstEntry & w.LastName == LastEntry);
                int properworker2 = properworker.Count();
                if (properworker2 == 0)
                {
                    //string newworker = ((Entry)sender).Text;
                    workers.FirstName = FirstEntry;
                    workers.LastName = LastEntry;
                    Globals.NFCtempcount++;
                    workers.ReferenceNFC = Globals.NFCtempcount.ToString();
                    workers.Company = currentcompany;

                    await App.Database.AddWorker(workers);

                    FirstNameText.Text = string.Empty;
                    LastNameText.Text = string.Empty;
                    CompanyNameText.Text = string.Empty;

                    SetWorkerList();
                }
                else
                {
                    await DisplayAlert("Error Worker Creation", "Worker Already Created", "Return to Entry");
                }
            }
            else
            {
                await DisplayAlert("Worker Entry Error", "Full name has not been entered", "Return to Entry");
            }
        }
        private async void OnMainNavClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
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