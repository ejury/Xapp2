using Plugin.NFC;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Data;
using Xapp2.Models;
using Xapp2.Pages.Popups;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewLoginPage : ContentPage
    {
        public NewLoginPage()
        {
            InitializeComponent();


            // Task.Run(AnimateBackground);
        }
        /*        private async void AnimateBackground()
                {
                    Action<double> forward = input => bdGradient.AnchorY = input;
                    Action<double> backward = input => bdGradient.AnchorY = input;

                    while (true)
                    {
                        bdGradient.Animate(name: "forward", callback: forward, start: 0, end: 1, length: 5000, easing: Easing.SinIn);
                        await Task.Delay(5000);
                        bdGradient.Animate(name: "backward", callback: backward, start: 1, end: 0, length: 5000, easing: Easing.SinIn);
                        await Task.Delay(5000);
                    }
                }*/

        private async void LoginClicked(object sender, EventArgs e)
        {
#if DEBUG //Verify internet connection for any new Db items for release code

#else
         {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await DisplayAlert("Error Admin Login", "Cannot signin as Admin without internet connection", "Obtain connection and retry");
                    return;
                }
            }
#endif
            var inputPopup = new AdminPopup();
            await PopupNavigation.Instance.PushAsync(inputPopup, true);
            var ret = await inputPopup.PopupClosedTask;
        }

        private async void SEClicked(object sender, EventArgs e)
        {
            //Get SE Badge Swipe
            var inputPopup = new BadgeReader();
            await PopupNavigation.Instance.PushAsync(inputPopup, true);
            var ret = await inputPopup.PopupClosedTask;
            string SEBadge = ret.SETag; 
            //Globals.NFCsignin = SEBadge;

            //Check if NFC scan was cancelled
            if (SEBadge == null)
            { return; }
            
            //Check if user wants to continue in offline mode when internet missing
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    bool answer = await DisplayAlert("Warning", "Internet connection not available. It is recommended to obtain connection and retry sign-in. Continue with sign-in in offline mode?", "Yes", "No");
                    if (answer)
                    {
                        //Pull database list
                        var workerlist = await App.Database.GetWorkers();

                        //Determine if login card is in Db
                        Worker activeworker = workerlist.Where(c => c.ReferenceNFC == SEBadge).ToList().FirstOrDefault();
                        if (activeworker == null)
                        {
                            await DisplayAlert("Login Error", "Card not active and/or valid. Return to Sign-in page and obtain internet connection", "Return");
                            return;
                        }
                        else
                        {
                            Globals.SELevel = activeworker.SELevel;
                            Globals.ServerName = "OFFLINE MODE";
                            Globals.UserDisplay = activeworker.LastName;
                            Globals.OfflineMode = true;
                        
                            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
                            return;
                        }
                    }
                    else
                    {
                    return; 
                    }
                }

            //Start Loading Icon
            AIndicator.IsRunning = true;

            //Create temp WorkerID for Web API
            Worker tempWorker = new Worker();
            tempWorker.ReferenceNFC = SEBadge;

            //Request JWT from Web API
            string IsValid = await APIServer.SEClient(tempWorker);

            //Check for API Error
            if (IsValid == "Card Not Active" | IsValid == "Unable to Sign In" | IsValid == "Invalid Card Used" | IsValid == "Server Error" | IsValid == "Server Connection Error")
            {
                DisplayAlert(IsValid, "The scanned card is not valid or has not been activited, please contact your administrator", "Return to login screen");
                AIndicator.IsRunning = false;
            }
            else
            {
                //Determine if new database selected and if local data clear required
                var workerlist = await App.Database.GetWorkers();
                string CurrentServ = workerlist.Select(c => c.ReferenceNFC).ToList().FirstOrDefault();
                string ServerName;
                try //Get new Servername
                {
                    int idx = tempWorker.ReferenceNFC.IndexOf('_');
                    ServerName = tempWorker.ReferenceNFC.Substring(0, idx);
                }
                catch  //Catch bad access card
                {
                    ServerName = null;
                }

                //Updating local data with new entries
                if (CurrentServ == ServerName) //Update local Db if accessing same server
                {
                    await App.Database.GetWorkersAPI();
                    await App.Database.GetUnitsAPI();
                    await App.Database.GetVesselsAPI();
                    await App.Database.GetLogsAPI();
                    await App.Database.GetAnalyticsAPI();
                }
                else //Clear local Db if new server accessed
                {
                    await App.Database.ClearLocal();
                    await App.Database.GetWorkersAPI();
                    await App.Database.GetUnitsAPI();
                    await App.Database.GetVesselsAPI();
                    await App.Database.GetLogsAPI();
                    await App.Database.GetAnalyticsAPI();
                }

                //await Navigation.PushModalAsync(new VesselEntryPage(), false).ConfigureAwait(false);
                await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
            }

            
        }

        private async void NFCClicked(object sender, EventArgs e)
        {
            var inputPopup = new SwipePopup();
            await PopupNavigation.Instance.PushAsync(inputPopup, true);
            var ret = await inputPopup.PopupClosedTask;
        }
    }
}   