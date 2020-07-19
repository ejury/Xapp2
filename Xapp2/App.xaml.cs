using System;
using Xamarin.Forms;
using Xapp2.Data;
using Xamarin.Forms.Xaml;
using System.IO;
using Xapp2.Models;

namespace Xapp2
{
    public partial class App : Application
    {
        static XDatabase database;

        public static XDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new XDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XApp2.db3"));
                }

                return database;
            }
        }



        public App()
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjAyMDg3QDMxMzcyZTM0MmUzMGlld0cxckhyMzV1bVpjZzNyZHBoblJsTWRadTM5UWJiMlpRSFd0ZW5Ia0k9");

            InitializeComponent();
            MainPage = new Xapp2.Pages.NewLoginPage();
            //MainPage = new MainPage();



        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
