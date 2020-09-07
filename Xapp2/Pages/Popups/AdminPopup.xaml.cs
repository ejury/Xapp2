using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Data;
using Xapp2.Models;
using Xapp2.Pages.Popups;

namespace Xapp2.Pages.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        private TaskCompletionSource<(bool isAccepted, string temp)> _taskCompletionSource;
        public Task<(bool isAccepted, string temp)> PopupClosedTask => _taskCompletionSource.Task;

      
        public AdminPopup()
        {
            InitializeComponent();

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
            ErrorMsg.Text = null;
            AIndicator.IsRunning = true;
            Credentials tempCred = new Credentials();
            tempCred.Email = UsernameEntry.Text;
            tempCred.Password = PasswordEntry.Text;
            tempCred.Server = ServerEntry.Text;

            bool SuccessFlag = false;
            
            try
            {
                string response = await APIServer.AdminClient(tempCred);

                if (response == "Unknown Error" | response == "Database Request Failed" | response == "User Not Found" | response == "Invalid Credentials")
                {
                    AIndicator.IsRunning = false;
                    ErrorMsg.Text=response;
                }
                else
                {
                    //Close popup
                    await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
                    await PopupNavigation.PopAsync();

                    
                }
            }
            catch 
            {   
                SuccessFlag = false; 
            }

        }

    }
}