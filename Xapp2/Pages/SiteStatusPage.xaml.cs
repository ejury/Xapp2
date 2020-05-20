using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SiteStatusPage : ContentPage
    {

        List<Vessel> vessellist = new List<Vessel>();
        List<Unit> unitlist = new List<Unit>();
        List<EntryLog> loglist = new List<EntryLog>();

        List<int> unitexpanded = new List<int>();
        int unitcount;
        bool hide;
        bool collapse;
        bool Nav;
        string space = "      ";

        protected override async void OnAppearing()
        {


            base.OnAppearing();
            await PullData();

            //Initialize DropDown Selection to logical 0 (no drop downs selected)
            unitcount = unitlist.Count;
            for (int i = 0; i < unitcount; i++)
            {
                unitexpanded.Add(0);
            }
            hide = false;

            SetViewList();


        }


        public SiteStatusPage()
        {
            Nav = true;
            InitializeComponent();

        }

        private async  Task PullData()
        {
            //Pulling current Database Heirarchy
            var test = await App.Database.GetVessels();
            vessellist = await App.Database.GetVessels();
            unitlist = await App.Database.GetUnits();
            loglist = await App.Database.GetLogs();

            return ;

        }

        private async void SetViewList()
        {
            //Generating listview templates
            List<StatusModel> statuslist = new List<StatusModel>();
            
            StatusModel image = new StatusModel(); image.path = "GreenButton.jpg";
            StatusModel noimage = new StatusModel(); noimage.path = "GrayButton.png";
            List<int> numberlist = new List<int>();

            // Index tool for # of vessels added to list
            int V = 0;


            //Populate Section of List for Each Unit/Vessel
            for (int i = 0; i < unitcount; i++)
            {

                if (unitexpanded[i] == 1) //If unit has been expanded
                {
                    StatusModel tempstatus = new StatusModel();
                    // Add unit line
                    tempstatus.droplist = i + " -";
                    tempstatus.namelist = unitlist[i].Name;

                    var activecount = loglist.Where(w => w.UnitName == unitlist[i].Name).ToList();
                    if (activecount.Any())
                    { tempstatus.path= "GreenButton.jpg"; }
                    else
                    { tempstatus.path= "GrayButton.png"; }

                    //numberlist.Add(activecount.Count);
                    tempstatus.numberlist = activecount.Count;

                    //Dont show row for inactive units/vessels if hide is active
                    if (hide)
                    {
                        if (tempstatus.path == "GreenButton.jpg")
                        {  statuslist.Add(tempstatus);  }
                    }
                    else
                    {  statuslist.Add(tempstatus);  }
                    
                    
                    // Add vessel line(s)
                    List<Vessel> vessellistunit = vessellist.Where(w => w.Unitname == unitlist[i].Name).ToList();
                    for (int t = 0; t < vessellistunit.Count; t++)
                    {
                        StatusModel tempstatus1 = new StatusModel();
                        tempstatus1.droplist = space;
                        tempstatus1.namelist = space + vessellistunit[t].Name;

                        activecount = loglist.Where(w => w.VesselName == vessellistunit[t].Name & w.UnitName == unitlist[i].Name).ToList();
                        if (activecount.Any())
                        { tempstatus1.path = "GreenButton.jpg"; }
                        else
                        { tempstatus1.path = "GrayButton.png"; }

                        tempstatus1.numberlist = activecount.Count;

                        //Dont show row for inactive units/vessels if hide is active
                        if (hide)
                        {
                            if (tempstatus1.path == "GreenButton.jpg")
                            { statuslist.Add(tempstatus1); }
                        }
                        else
                        { statuslist.Add(tempstatus1); }
                    }
                }
                else  //If unit has not been expanded
                {
                    StatusModel tempstatus = new StatusModel();
                    tempstatus.droplist= i+" +";
                    // Add unit line
                    tempstatus.namelist=unitlist[i].Name;

                    var activecount = loglist.Where(w => w.UnitName == unitlist[i].Name).ToList();
                    if (activecount.Any())
                    { tempstatus.path = "GreenButton.jpg"; }
                    else
                    { tempstatus.path = "GrayButton.png"; }

                    tempstatus.numberlist= activecount.Count;

                    //Dont show row for inactive units/vessels if hide is active
                    if (hide)
                    {
                        if (tempstatus.path == "GreenButton.jpg")
                        { statuslist.Add(tempstatus); }
                    }
                    else
                    { statuslist.Add(tempstatus); }
                }
            }
            statusview.ItemsSource = statuslist;

        }
        private async void DropDownSelected(object sender, EventArgs e)
        {
            // Generate filtered unit list (ex vessel tags)
            List<StatusModel> list = (List<StatusModel>)statusview.ItemsSource;
            List<string> droplist = list.Select(c => c.droplist).ToList();
            for (int i = droplist.Count - 1; i > -1; i--)
            {
                string temp = droplist[i];
                if (temp == space)
                {
                    droplist.RemoveAt(i);
                }
            }
            // Switch expansion variable for selected unit
            StatusModel selectedmodel = (StatusModel)statusview.SelectedItem;
            string selectedunit = (string)selectedmodel.droplist;
            int index = droplist.IndexOf(selectedunit);

            if (selectedunit.EndsWith("+"))
            {
                unitexpanded[index] = 1;
                SetViewList();
            }
            if (selectedunit.EndsWith("-"))
            {
                unitexpanded[index] = 0;
                SetViewList();
            }
            if (selectedunit.EndsWith(" "))
            {
                string answer = await DisplayActionSheet("Vessel Selection Options", "Cancel", null, "Open CSE Manager", "Open Analytics");

                if (answer == null)
                {
                }
                else
                {
                    //Update global location selection prior to page navigation
                    Globals.init = true;
                    Globals.vessel = selectedmodel.namelist.Remove(0, space.Length); //grabs vessel name of selected row and trims indent spacers

                    //Determine unitname of vessel (without database query) search text list
                    List<string> namelist = list.Select(c => c.namelist).ToList();
                    index = namelist.IndexOf(selectedmodel.namelist);

                    int track = 0;
                    while (track == 0) //track upwards through list to find unitname
                    {
                        index--;
                        if (namelist[index].First() != ' ')
                        {
                            Globals.unit = namelist[index];
                            track = 1;
                        }
                    }

                    //Open requested page navigation
                    if (answer == "Open CSE Manager")
                    {
                        await Navigation.PushModalAsync(new CSEntryPage(), false);
                    }
                    if (answer == "Open Analytics")
                    {
                        await Navigation.PushModalAsync(new AnalyticsPage(), false);
                    }
                }
            }
        }
        private async void OnHideClicked(object sender, EventArgs e)
        {
            if (hide)
            {
                hide = false;
                HideButton.Text = "Hide";
                SetViewList();
            }
            else
            {
                hide = true;
                HideButton.Text = "Unhide";
                SetViewList();
            }
        }
        private async void OnCollapseClicked(object sender, EventArgs e)
        {
            if (collapse)
            {
                collapse = false;
                CollapseButton.Text = "Expand";
                for (int i=0; i<unitexpanded.Count; i++)
                { unitexpanded[i] = 0; }
                SetViewList();
            }
            else
            {
                collapse = true;
                CollapseButton.Text = "Collapse";
                for (int i = 0; i < unitexpanded.Count; i++)
                { unitexpanded[i] = 1; }
                SetViewList();
            }
        }
        private async void LineSelected(object sender, EventArgs e)
        {
        }
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await PullData();
            SetViewList();

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