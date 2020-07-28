using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Data;
using Xapp2.Models;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewLoginPage : ContentPage
    {
        public NewLoginPage()
        {
            InitializeComponent();
        }

        private async void LoginClicked(object sender, EventArgs e) 
        {
            Credentials tempCred = new Credentials();
            tempCred.Email = UserEntry.Text;
            tempCred.Password = PasswordEntry.Text;

            await APIServer.RegClient(tempCred);

    
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
        }

        private async void SEClicked(object sender, EventArgs e)
        {
            Worker tempWorker = new Worker();
            tempWorker.ReferenceNFC = UserEntry.Text;
            
            string IsValid = await APIServer.SEClient(tempWorker);

            if (IsValid == "Card Not Active")
            {
                DisplayAlert("Card Error", "The scanned card is not valid or has not been activited, please contact your administrator", "Return to login screen");
            }
            else
            {
                await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
            }


        }
    }
}