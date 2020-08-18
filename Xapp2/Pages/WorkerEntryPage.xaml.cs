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
            List<string> companylist = companylistlong.Distinct().ToList();

            

            if (companylist.Count > 0 & workerlist.Count > 0)
            {
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
            List<string> companylistlong = workerlist.Select(c => c.Company).ToList();
            List<string> companylist = companylistlong.Distinct().ToList();

            FilterWorkerList();

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
        private async void FilterWorkerList()
        {
            //Update worker list for the current selected company
            var workerlist = await App.Database.GetWorkers();
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
/*        async void OnManualCompanyChange(object sender, EventArgs e)
        {
            string ManualCompanyEntry = CompanyNameText.Text;
            if (ManualCompanyEntry.Length > 0)
            {
                companypicker.SelectedItem = string.Empty;
            }
        }*/
        async void NewWorkerClicked(object sender, EventArgs e)
        {
            
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
                    workers.ReferenceNFC = TempSECardEntry.Text;
                    workers.Company = currentcompany;
                    workers.CreatedTime = DateTime.Now;

                    //Add worker to database
                    int ValidCard = await App.Database.AddWorker(workers);
                    if (ValidCard==0)//Card already active flag from dB
                    {
                        await DisplayAlert("Error Worker Creation", "Card already assigned to a user", "Return to Entry");
                    }
                    if (ValidCard==-1)//Card not valid in dB
                    {
                        await DisplayAlert("Error Worker Creation", "Invalid Card Used", "Return to Entry");
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
            Worker LineSelected = (Worker)workersview.SelectedItem;

            if (LineSelected != null) //Don't display options if deselecting was trigger
            {
                string answer = await DisplayActionSheet("User Selection Options", "Cancel", null, "Delete User");

                if (answer == "Delete User")
                {
                    bool answer2 = await DisplayAlert("Delete selected user?", "Confirmation", "Yes", "No");
                    if (answer2)
                    {
                        //Delete selected worker and reset displayed list
                        await App.Database.DeleteWorker(LineSelected.WorkerID);
                        SetWorkerList();
                    }
                }
            }
        }
        private async void OnMainNavClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
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