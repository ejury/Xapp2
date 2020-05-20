﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        // Task<List<Vessel>> _fetchingUnits;
        Vessel vessels = new Vessel();
        Unit units = new Unit();
        UnitPieView pieview = new UnitPieView();
        int currentselect;
        bool Nav;

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Create Initial Unit List Selection
            var unitslist = await App.Database.GetUnits();
            var vessellist = await App.Database.GetVessels();
            if (unitslist.Count>0 & vessellist.Count > 0)
            {
                picker.ItemsSource = unitslist;


                //Set Initial Unit Selection
                currentselect = 0;
                pieview.pieselect = currentselect;
                picker.SelectedIndex = currentselect;
                currentunit = unitslist[currentselect].Name;
                //SetVesselList();
            }
        }
        private async void SetUnitList()
        {
            var unitslist = await App.Database.GetUnits();
            picker.ItemsSource = unitslist;
            picker.SelectedIndex = pieview.pielabels.Count - 1;
        }
            private async void SetVesselList()
        {
            //Pulling current Database Heirarchy
            var vessellist = await App.Database.GetVessels();
            var unitslist = await App.Database.GetUnits();
            
            //Setting vessellist per current unit selection
            var vessellistFilter = vessellist.Where(w => w.Unitname == currentunit); //Filter for vessels in selected unit
            List<Vessel> v22 = (List < Vessel>)vessellistFilter.ToList();
            
            //Split list of vessels into two columns
            int NumOfVes = vessellistFilter.Count();
            IEnumerable<Vessel> v1; IEnumerable<Vessel> v2;
            List<string> u1 = new List<string>();
            if (NumOfVes%2 == 0)// number is even
            {
                v1 = v22.Take(NumOfVes / 2);
                v2 = (v22.Skip(NumOfVes / 2).Take(NumOfVes / 2));
            }
            else
            {
                v1 = v22.Take((NumOfVes+1) / 2);
                v2 = (v22.Skip((NumOfVes +1)/2).Take(NumOfVes-1 / 2));
            }
            vesselview1.ItemsSource = v1;
            vesselview2.ItemsSource = v2;

            var temp = currentunit;
            unitlabelname.Text = currentunit;

            //Generating Pie Chart
            int t = unitslist.Count;
            UnitPieModel temppie = new UnitPieModel();
            Unit tempunit = new Unit();

            pieview.piemodel.Clear();   //Clear old entries
            pieview.pielabels.Clear();  //Clear old entries
            for (int i=0;i<t;i++)
            { 
                int matchunitname = vessellist.Where(w => w.Unitname == unitslist[i].Name).Count();
                tempunit = unitslist[i];
                temppie.Unitname = tempunit.Name;
                temppie.Vesselcount = matchunitname;


                //Updating Pie Chart
                pieview.piemodel.Add(temppie);
                pieview.pielabels.Add(unitslist[i].Name);
            }
            pie.ExplodeIndex = pieview.pieselect;
    }

        public VesselEntryPage()
        {
            Nav = true;
            InitializeComponent();   
            BindingContext = pieview;   //Pie Chart Context
        }
        async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (pieview.pieselect != currentselect)
            {
                currentselect = pieview.pieselect;
                picker.SelectedIndex = pieview.pieselect;
            }
            if (picker.SelectedIndex != currentselect)
            {
                currentselect = picker.SelectedIndex;
                pieview.pieselect = picker.SelectedIndex;
            }

            //Updating vessel list display
            if (pieview.pieselect != -1) //Ensure a value has been selected
            {
                var test = (Unit)picker.ItemsSource[pieview.pieselect];
                currentunit = test.Name;
            }
            SetVesselList();
        }

        //async void PieSelectChange(object sender, EventArgs e)
        //{
        //    //var picker = sender;
        //    var unitslist = await App.Database.GetUnits();
        //    if (pieview.pieselect < unitslist.Count & pieview.pieselect > -1)
        //    {
        //        //Updating vessel list display
                
        //        currentunit = unitslist[pieview.pieselect].Name;
        //        picker.SelectedIndex = pieview.pieselect;
        //    }
        //    else
        //    {
        //        int t = 1;
        //    }
        //}

        async void OnUnitEntryCompleted(object sender, EventArgs e)
        {
            string newunit = ((Entry)sender).Text;
            units.Name = newunit;

            await App.Database.AddUnit(units);
            currentunit = units.Name;
            SetUnitList();

            UnitEntryText.Text = string.Empty;
            
        }
        async void OnVesselEntryCompleted(object sender, EventArgs e)
        {
            if (currentunit.Length > 0)
            {
                var unitslist = await App.Database.GetUnits();
                var properunit = unitslist.Where(w => w.Name == currentunit);
                int properunit2 = properunit.Count();
                if (properunit2 > 0)
                {
                    string newvessel = ((Entry)sender).Text;
                    vessels.Name = newvessel;
                    vessels.Unitname = currentunit;

                    await App.Database.AddVessel(vessels);

                    SetVesselList();
                    //_fetchingUnits = App.Database.GetVessels();
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
        async void VesselSelected2(object sender, EventArgs e)
        {
            //Grabbing selected Vessel Object
            Vessel LineSelected = (Vessel)vesselview2.SelectedItem;

            if (LineSelected != null) //Don't display options if deselecting was trigger
            {
                VesselSelectOptions(2, LineSelected);
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
            vesselview2.SelectedItem = null;
        }

        async void DeleteUnitSelected(object sender, EventArgs e) //////////
        {
            bool answer = await DisplayAlert("Delete selected unit?", "Confirmation", "Yes", "No");
            if (answer)
            {
                var deleteunit = picker.SelectedItem;
                Unit deleteunit2 = new Unit();
                deleteunit2 = (Unit)deleteunit;
                await App.Database.DeleteUnit(deleteunit2.UnitID);
                currentunit = "";
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