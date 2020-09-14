using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var inputPopup = new AdminPopup();
            await PopupNavigation.Instance.PushAsync(inputPopup, true);
            var ret = await inputPopup.PopupClosedTask;
        }

        private async void SEClicked(object sender, EventArgs e)
        {
            //Start Loading Icon
            AIndicator.IsRunning = true;

            //Create temp WorkerID for Web API
            Worker tempWorker = new Worker();
            tempWorker.ReferenceNFC = UserEntry.Text;

            //Request JWT from Web API
            string IsValid = await APIServer.SEClient(tempWorker);

            //Check for API Error
            if (IsValid == "Card Not Active" | IsValid == "Unable to Sign In" | IsValid == "Invalid Card Used" | IsValid == "Server Error")
            {
                DisplayAlert(IsValid, "The scanned card is not valid or has not been activited, please contact your administrator", "Return to login screen");
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

                await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
            }

            AIndicator.IsRunning = true;
        }
    }
}   