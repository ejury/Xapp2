using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xapp2.Pages;
using Xapp2.Views.LoginPage;

namespace Xapp2
{

    [DesignTimeVisible(false)]
    
    public partial class MainPage : ContentPage
    {
        bool Nav;
        public MainPage()
        {
            Nav = true;
            InitializeComponent();
        }
        private async void OnDatabaseRefresh(object sender, EventArgs e)
        {

             bool answer = await DisplayAlert("Reset Full Database?", "Confirmation", "Yes", "No");
            if (answer)
            {
                await App.Database.ClearVessel();
                await App.Database.ClearUnit();
                await App.Database.ClearWorker();
                await App.Database.ClearLogs();
                await App.Database.ClearAnalytics();
                await App.Database.RefreshDatabase();
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
            }
        }
        private async void OnLogout(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NewLoginPage(), false).ConfigureAwait(false);
        }
        #region Navbar
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
            if(!Nav)
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
            CSEButton.Opacity = .5;
            await Navigation.PushModalAsync(new CSEntryPage(), false);

        }
        private async void OnSiteStatusButtonClicked(object sender, EventArgs e)
        {
            StatusButton.Opacity = .5;
            await Navigation.PushModalAsync(new SiteStatusPage(), false);
        }
        private async void OnVesselButtonClicked(object sender, EventArgs e)
        {
            HeirarchyButton.Opacity = .5;
            await Navigation.PushModalAsync(new VesselEntryPage(), false).ConfigureAwait(false);
        }
        private async void OnWorkerButtonClicked(object sender, EventArgs e)
        {
            WorkerButton.Opacity = .5;
            await Navigation.PushModalAsync(new WorkerEntryPage(), false).ConfigureAwait(false);
        }
        private async void OnAnalyticsButtonClicked(object sender, EventArgs e)
        {
            AnalyticsButton.Opacity = .5;
            await Navigation.PushModalAsync(new AnalyticsPage(), false).ConfigureAwait(false);
        }

        #endregion
    }
}
