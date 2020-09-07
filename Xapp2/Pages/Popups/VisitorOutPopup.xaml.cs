using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;
using Xapp2.Pages.Popups;

namespace Xapp2.Pages.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VisitorOutPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        private TaskCompletionSource<(bool isAccepted, string temp)> _taskCompletionSource;
        public Task<(bool isAccepted, string temp)> PopupClosedTask => _taskCompletionSource.Task;

        List<TimeDisplay> visitors;

        public VisitorOutPopup(List<TimeDisplay> timedisplay)
        {
            InitializeComponent();
            visitors = timedisplay;
            BindingContext = visitors;
            //visitorsview.BindingContext = visitors;
            visitorsview.ItemsSource = timedisplay;
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

        private async void WorkerExitSelected(object sender, EventArgs e)
        {
            TimeDisplay selectedindex = (TimeDisplay)visitorsview.SelectedItem;
            await App.Database.DeleteLog(selectedindex.EntryID);

            //TimeDisplay selected = visitors.Find(i => i.EntryID == selectedindex.EntryID);
            visitors.Remove(selectedindex);
            visitorsview.ItemsSource = null;
            visitorsview.ItemsSource = visitors;

        }
        private async void Button_OnClicked(object sender, EventArgs e)
        {
            //Close popup
            await PopupNavigation.PopAsync();

        }
    }
}