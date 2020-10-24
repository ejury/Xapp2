using Plugin.NFC;
using System;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.ComponentModel;
using Rg.Plugins.Popup.Services;

namespace Xapp2.Pages.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BadgeReader : Rg.Plugins.Popup.Pages.PopupPage
    {
        private TaskCompletionSource<(bool isAccepted, string SETag)> _taskCompletionSource;
        public Task<(bool isAccepted, string SETag)> PopupClosedTask => _taskCompletionSource.Task;

        //NFC Variables
        public const string ALERT_TITLE = "NFC";
        public const string MIME_TYPE = "application/com.companyname.Jurisoft";

        NFCNdefTypeFormat _type;
        bool _makeReadOnly = false;
        bool _eventsAlreadySubscribed = false;
		bool ChkReadOnly = false;

		private bool _nfcIsEnabled;
        public bool NfcIsEnabled
        {
            get => _nfcIsEnabled;
            set
            {
                _nfcIsEnabled = value;
                OnPropertyChanged(nameof(NfcIsEnabled));
                OnPropertyChanged(nameof(NfcIsDisabled));
            }
        }

        public BadgeReader()
        {
            InitializeComponent();

        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            _taskCompletionSource = new TaskCompletionSource<(bool isAccepted, string temp)>();

			if (CrossNFC.IsSupported)
            {
                if (!CrossNFC.Current.IsAvailable)
                    await ShowAlert("NFC is not available");

                NfcIsEnabled = CrossNFC.Current.IsEnabled;
                if (!NfcIsEnabled)
                    await ShowAlert("NFC is disabled");

                SubscribeEvents();

                await StartListeningIfNotiOS();
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
			UnsubscribeEvents(); //Ensure NFC queries are reset on leaving page
			_taskCompletionSource.TrySetResult((true, null));

		}

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            AIndicator.IsRunning = false;
			CrossNFC.Current.StopListening();
			//Close popup
			await PopupNavigation.PopAsync();
		}
		private async void TempNFCcomplete(object sender, EventArgs e)
		{
			string temp = TempNFC.Text;
			_taskCompletionSource.SetResult((true,temp));

			//Close popup
			await PopupNavigation.PopAsync();
		}


		//NFC Specific 
		private async Task NFCcode(string ReadTag)
        {
			_taskCompletionSource.SetResult((true, ReadTag));

			//Close popup
			await PopupNavigation.PopAsync();
		}


		////////////////////////// NFC CODE //////////////////////////////////

		public bool NfcIsDisabled => !NfcIsEnabled;

		protected override bool OnBackButtonPressed()
		{
			UnsubscribeEvents();
			CrossNFC.Current.StopListening();
			return base.OnBackButtonPressed();
		}

		/// Subscribe to the NFC events
		void SubscribeEvents()
		{
			if (_eventsAlreadySubscribed)
				return;

			_eventsAlreadySubscribed = true;

			CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
			CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;

			if (Device.RuntimePlatform == Device.iOS)
				CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
		}

		/// Unsubscribe from the NFC events
		void UnsubscribeEvents()
		{
			CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
			CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

			if (Device.RuntimePlatform == Device.iOS)
				CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;
		}

		/// Event raised when NFC Status has changed
		public async void Current_OnNfcStatusChanged(bool isEnabled)
		{
			NfcIsEnabled = isEnabled;
			await ShowAlert($"NFC has been {(isEnabled ? "enabled" : "disabled")}");
		}

		/// Event raised when a NDEF message is received
		public async void Current_OnMessageReceived(ITagInfo tagInfo)
		{
			if (tagInfo == null)
			{
				await ShowAlert("No tag found");
				return;
			}

			// Customized serial number
			var identifier = tagInfo.Identifier;
			var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
			var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"Tag [{serialNumber}]" : "Tag Info";

			if (!tagInfo.IsSupported)
			{
				await ShowAlert("Unsupported tag (app)", title);
			}
			else if (tagInfo.IsEmpty)
			{
				await ShowAlert("Empty tag", title);
			}
			else
			{
				var first = tagInfo.Records[0];
				await NFCcode(first.Message);
			}
		}

		/// Event raised when user cancelled NFC session on iOS 
		void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) => Debug("User has cancelled NFC Session");
	
		/// Write a debug message in the debug console
		void Debug(string message) => System.Diagnostics.Debug.WriteLine(message);

		/// Display an alert
		Task ShowAlert(string message, string title = null) => DisplayAlert(string.IsNullOrWhiteSpace(title) ? ALERT_TITLE : title, message, "Cancel");

		/// Task to start listening for NFC tags if the user's device platform is not iOS
		async Task StartListeningIfNotiOS()
		{
			if (Device.RuntimePlatform == Device.iOS)
				return;
			await BeginListening();
		}

		/// Task to safely start listening for NFC Tags
		async Task BeginListening()
		{
			try
			{
				CrossNFC.Current.StartListening();
				AIndicator.IsRunning = true;
			}
			catch (Exception ex)
			{
				await ShowAlert(ex.Message);
			}
		}

	}
}