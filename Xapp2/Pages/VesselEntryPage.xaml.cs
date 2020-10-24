using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;
using Xapp2.Models.ViewModel;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VesselEntryPage : ContentPage
    {
        public string currentunit;
        Vessel vessels = new Vessel();
        Unit units = new Unit();
        UnitPieView pieview = new UnitPieView();
        int currentselect;
        bool Nav;
        int ListViewMode = 2; //Bool property for what view is displayed

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Create Initial Unit List Selection
            List<Vessel> vessellist = await App.Database.GetVessels();
            List<string> unitslist = await orderunits(vessellist);

            if (unitslist.Count>0 & vessellist.Count > 0)
            {

                picker.ItemsSource = unitslist;

                //Set Initial Unit Selection
                currentselect = 0;
                pieview.pieselect = currentselect;
                picker.SelectedIndex = currentselect;
                currentunit = unitslist[currentselect];

                // Populating display if data is present
                if (vessellist.Count() > 0)
                {
                    await SetVesselList();
                    //ListFrame.HeightRequest = PieFrame.Height; //set listview size
                }
            }
        }

        public async Task<List<string>> orderunits (List<Vessel> vessellist)
        {
            
            List<string> unitslist = vessellist.Select(c => c.Unitname).ToList() //Order from largest to smallest
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .ToList();

            if (unitslist.Count > 0)
            {
                // Add any units with zero locations before populating list
                var unitslist2 = await App.Database.GetUnits();
                for (int t = 0; t < unitslist2.Count(); t++)
                {
                    if (!unitslist.Contains(unitslist2[t].Name))
                    {
                        unitslist.Add(unitslist2[t].Name);
                    }
                }
            }
            return unitslist;
        }
        private async void SetUnitList()
        {
            List<Vessel> vessellist = await App.Database.GetVessels();
            List<string> unitslist = await orderunits(vessellist);
            picker.ItemsSource = unitslist;
            picker.SelectedIndex = pieview.pielabels.Count - 1;
        }
            private async Task SetVesselList()
        {
            //Pulling current Database Heirarchy
            List<Vessel> vessellist = await App.Database.GetVessels();
            List<string> unitslist = await orderunits(vessellist);

            if (vessellist == null)
            {
                vesselview1.ItemsSource = null;

            }
            else
            {
                //Setting vessellist per current unit selection
                var vessellistFilter = vessellist.Where(w => w.Unitname == currentunit); //Filter for vessels in selected unit
                List<Vessel> v22 = (List<Vessel>)vessellistFilter.ToList();
                vesselview1.ItemsSource = v22;
            }

            var temp = currentunit;
            unitlabelname.Text = currentunit;

            //Generating Pie Chart
            int t = unitslist.Count;
            UnitPieModel temppie = new UnitPieModel();

            pieview.piemodel.Clear();   //Clear old entries
            pieview.pielabels.Clear();  //Clear old entries
            for (int i=0;i<t;i++)
            { 
                int matchunitname = vessellist.Where(w => w.Unitname == unitslist[i]).Count();
                temppie.Unitname = unitslist[i];
                temppie.Vesselcount = matchunitname;

                //Updating Pie Chart
                pieview.piemodel.Add(temppie);
                pieview.pielabels.Add(unitslist[i]);
            }
            pie.ExplodeIndex = pieview.pieselect;
    }

         public VesselEntryPage()
        {

            Nav = true;
            InitializeComponent();
            BindingContext = pieview;   //Pie Chart Context

        }
        void PieChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExplodeIndex") //Only trigger update for new chart selection (Optimization)
            {
                OnPickerSelectedIndexChanged(sender, e);
            }
        }
        async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            //Error handler due to events during page changes. Verifies both pickers can be accessed and skips updates if models not loaded
            int selectedIndex = -1;
            try
            {
                if (picker.SelectedIndex != -1) 
                    { selectedIndex = picker.SelectedIndex; }
                if (pieview.pieselect != -1)
                    { selectedIndex = pieview.pieselect; }
            }
            catch (Exception e1)
            {
                selectedIndex = -1;
            }

            int changed = 0; //test if company selection state has changed to avoid circular loop

            //Updating vessel list display
            if (selectedIndex != -1) //Ensure a value has been selected
            {
                //Updating company selection state variables
                if (pieview.pieselect != currentselect & pieview.pieselect != -1)
                {
                    currentselect = pieview.pieselect;
                    picker.SelectedIndex = pieview.pieselect;
                    changed = 1;
                }
                if (picker.SelectedIndex != currentselect & picker.SelectedIndex != -1)
                {
                    currentselect = picker.SelectedIndex;
                    pieview.pieselect = picker.SelectedIndex;
                    changed = 1;
                }

                if (changed ==1 ) //Update list if a state change has occurred
                {
                    currentunit = pieview.pielabels[currentselect];
                    SetVesselList();
                }
            }
        }
        private void ListViewChange(object sender, EventArgs e)
        {
            //Move listview mode through 3 states - 1=both visible, 2=pie visible, 3=list visible
            if (ListViewMode > 1)
            { ListViewMode--; }
            else
            { ListViewMode = 3; }

            //Setting appropriate view state
            if (ListViewMode == 1)
            {
                LView1.TextColor = Color.Aqua;
                RView1.TextColor = Color.Aqua;
                RView2.TextColor = Color.Aqua;

                PieFrameLegend.IsVisible = false;
                LDisplay.Width= GridLength.Star;
                RDisplay.Width = GridLength.Star;
            }
            if (ListViewMode == 2)
            {
                LView1.TextColor = Color.Aqua;
                RView1.TextColor = Color.White;
                RView2.TextColor = Color.White;

                PieFrameLegend.IsVisible = true;
                LDisplay.Width = GridLength.Star;
                RDisplay.Width = 0;
            }
            if (ListViewMode == 3)
            {
                LView1.TextColor = Color.White;
                RView1.TextColor = Color.Aqua;
                RView2.TextColor = Color.Aqua;

                LDisplay.Width = 0;
                RDisplay.Width = GridLength.Star;
            }

            SetVesselList();
        }
        async void OnUnitEntryCompleted(object sender, EventArgs e)
        {
            if (Globals.SELevel > 1)
            {
                await DisplayAlert("Unauthorized", "User does not have authorization for scanning functionality. Contact your administrator", "Return to Portal");
                return;
            }

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
            units.Name = UnitEntryText.Text;

            await App.Database.AddUnit(units);
            currentunit = units.Name;
            SetUnitList();

            UnitEntryText.Text = string.Empty;
            
        }
        async void OnVesselEntryCompleted(object sender, EventArgs e)
        {
            if (Globals.SELevel > 1)
            {
                await DisplayAlert("Unauthorized", "User does not have authorization for scanning functionality. Contact your administrator", "Return to Portal");
                return;
            }
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
            if (currentunit.Length > 0)
            {
                var unitslist = await App.Database.GetUnits();
                var properunit = unitslist.Where(w => w.Name == currentunit);
                int properunit2 = properunit.Count();
                if (properunit2 > 0)
                {
                    vessels.Name = VesselEntryText.Text;
                    vessels.Unitname = currentunit;

                    await App.Database.AddVessel(vessels);

                    SetVesselList();
                    VesselEntryText.Text = string.Empty;
                }
                else
                {
                    await DisplayAlert("Error Unit Selection", "Invalid Unit Selection", "Return to Entry");
                    VesselEntryText.Text = string.Empty;
                }
            }
            else
            {
                await DisplayAlert("Error Unit Selection", "Invalid Unit Selection", "Return to Entry");
                VesselEntryText.Text = string.Empty;
            }

        }
        async void VesselSelected1(object sender, EventArgs e) 
        {
            //Grabbing selected Vessel Object
            Vessel LineSelected = (Vessel)vesselview1.SelectedItem;

            if (LineSelected != null) //Don't display options if deselecting was trigger
            {
                VesselSelectOptions(1, LineSelected);
            }
        }
        async void VesselSelectOptions(int column, Vessel LineSelected)
        {
            string answer = await DisplayActionSheet("Vessel Selection Options", "Cancel", null, "Open CSE Manager", "Open Analytics", "Delete Vessel");

            if (answer == "Delete Vessel")
            {
                bool answer2 = await DisplayAlert("Delete selected vessel?", "Confirmation", "Yes", "No");
                if (answer2)
                {
                    //Delete selected vessel and reset displayed list
                    await App.Database.DeleteVessel(LineSelected.VesselID);
                    SetVesselList();
                }
            }
            if (answer == "Open CSE Manager")
            {
                //Updating global selection
                Globals.init = true;
                Globals.vessel = LineSelected.Name;
                Globals.unit = LineSelected.Unitname;

                //Navigate to CSE Page
                await Navigation.PushModalAsync(new CSEntryPage(), false);
            }
            if (answer == "Open Analytics")
            {
                //Updating global selection
                Globals.init = true;
                Globals.vessel = LineSelected.Name;
                Globals.unit = LineSelected.Unitname;

                //Navigate to CSE Page
                await Navigation.PushModalAsync(new AnalyticsPage(), false);
            }
            //Clear selected items
            vesselview1.SelectedItem = null;
        }

        async void DeleteUnitSelected(object sender, EventArgs e) //////////
        {
            bool answer = await DisplayAlert("Delete selected unit?", "Confirmation", "Yes", "No");
            if (answer)
            {
                var deleteunit = picker.SelectedItem;
                Unit deleteunit2 = new Unit();
                deleteunit2 = (Unit)deleteunit;
                await App.Database.DeleteUnit(deleteunit2);
                currentunit = "";
                SetUnitList();
            }
        }

        private async void OnDatabaseRefresh(object sender, EventArgs e)
        {

            bool answer = await DisplayAlert("Reset Full Database?", "Confirmation", "Yes", "No");
            if (answer)
            {
                AIndicator.IsRunning=true;
                await App.Database.ClearVessel();
                await App.Database.ClearUnit();
                await App.Database.ClearWorker();
                await App.Database.ClearLogs();
                await App.Database.ClearAnalytics();
                await App.Database.RefreshDatabase();
                AIndicator.IsRunning = false;
                SetUnitList();
            }
        }
        private async void OnDatabaseClear(object sender, EventArgs e)
        {

            bool answer = await DisplayAlert("Reset Full Database?", "Confirmation", "Yes", "No");
            if (answer)
            {
                await App.Database.ClearVessel();
                await App.Database.ClearUnit();
                await App.Database.ClearWorker();
                await App.Database.ClearLogs();
                await App.Database.ClearAnalytics();
                SetUnitList();
            }
        }
        private async void OnMainNavClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
        }

        private void PieSeries_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

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
    }
}