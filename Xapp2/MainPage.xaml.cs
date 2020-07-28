using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xapp2.Models;
using Xapp2.Models.ViewModel;
using Xapp2.Pages;
using Xapp2.Views.LoginPage;

namespace Xapp2
{

    [DesignTimeVisible(false)]
    
    public partial class MainPage : ContentPage
    {
        //Analytics List References
        List<DateTimeRange> chartdata = new List<DateTimeRange> { new DateTimeRange(), new DateTimeRange() };
        List<AnalyticsLog> logs = new List<AnalyticsLog>();
        DateTime start = DateTime.Now.AddDays(-2);
        DateTime finish = DateTime.Now;
        bool Nav;
        public MainPage()
        {
            Nav = true;
            InitializeComponent();
            SetAnalyticsList();
        }


        private async Task<int> SetAnalyticsList()
        {
            // Pull and filter all entry logs on specific location
            logs = await App.Database.GetAnalytics();

            logs.Sort((x, y) => DateTime.Compare(x.TimeLog, y.TimeLog));

            if (logs.Count > 0) //only generate chart data if logs exist
            {

                chartdata[0].DateTimeData.Clear();
                chartdata[1].DateTimeData.Clear();
                int secondseries = 1;


                for (int k = 0; k < secondseries; k++)
                {
                    int markers = 0; //tracks number of datapoint in series

                    if (logs.Count > 0)
                    {

                        for (int z = 0; z < logs.Count; z++)
                        {

                            LogModel temprange = new LogModel(); //New graphical point at different Y point
                            LogModel temprange2 = new LogModel();//Carry old Y value to new X value to create step effect

                            temprange.Date = logs[z].TimeLog;
                            temprange2.Date = logs[z].TimeLog;
                            temprange.Company = logs[z].Company;
                            temprange2.Company = logs[z].Company;

                            if (chartdata[k].DateTimeData.Count == 0)
                            {
                                temprange.Value = 1;
                                temprange2.Value = 0;
                            }
                            else
                            {
                                temprange2.Value = chartdata[k].DateTimeData[(2 * markers) - 1].Value;

                                if (logs[z].InOut == 1)
                                {
                                    temprange.Value = chartdata[k].DateTimeData[(2 * markers) - 1].Value + 1;
                                }
                                else
                                {
                                    temprange.Value = chartdata[k].DateTimeData[(2 * markers) - 1].Value - 1;
                                }
                            }

                            if (k == 0)
                            {
                                chartdata[k].DateTimeData.Add(temprange2);
                                chartdata[k].DateTimeData.Add(temprange);
                                markers++;
                            }
/*                            if (k == 1 & temprange.Company == CSeriesSelect[1])
                            {
                                chartdata[k].DateTimeData.Add(temprange2);
                                chartdata[k].DateTimeData.Add(temprange);
                                markers++;
                            }*/
                        }
                    }
                }
/*                if (start.Year == 1)
                {
                    start = chartdata[0].DateTimeData[0].Date;
                    finish = chartdata[0].DateTimeData[2 * logs.Count - 1].Date;
                }*/
                //chart data
                SendToChart();
            }
            else { DisplayAlert("Data Selection Error", "The selected location does not have any past entry activity", "Return to selection"); }
            return 1;
        }
        private async void SendToChart()
        {
            // Create temporary chartdata lists with cropped data (All companys, selected company, blank data reference)
            List<DateTimeRange> chartdata2 = new List<DateTimeRange> { new DateTimeRange(), new DateTimeRange(), new DateTimeRange() };


            // if (start < chartdata[0].DateTimeData[0].Date) 
            {
                LogModel temprange = new LogModel();
                temprange.Date = start;
                temprange.Value = 0;
                chartdata2[0].DateTimeData.Add(temprange);
            }

            // Determine # of series
            int num = 1;

            //Reducing series size based on range selection
            for (int k = 0; k < num; k++)
            {
                for (int i = 0; i < chartdata[k].DateTimeData.Count; i++)
                {
                    if (chartdata[k].DateTimeData[i].Date >= start & chartdata[k].DateTimeData[i].Date <= finish)
                    {
                        chartdata2[k].DateTimeData.Add(chartdata[k].DateTimeData[i]);
                    }
                }

                //Adding start/end datapoints to ensure requested range is shown
                if (finish > chartdata[k].DateTimeData[chartdata[k].DateTimeData.Count - 1].Date)
                {
                    LogModel temprange = new LogModel();
                    temprange.Date = chartdata[k].DateTimeData[chartdata[k].DateTimeData.Count - 1].Date;
                    temprange.Value = 0;
                    chartdata2[k].DateTimeData.Add(temprange);

                    LogModel temprange2 = new LogModel();
                    temprange2.Date = finish;
                    temprange2.Value = 0;
                    chartdata2[k].DateTimeData.Add(temprange2);
                }
            }

            // Clear charts
            //sfchart.ItemsSource = chartdata2[2].DateTimeData;
            //Displaying series if selected

                sfchart.ItemsSource = chartdata2[0].DateTimeData;



            // Formatting graph datetime grid layout
            if (finish.DayOfYear - start.DayOfYear > 3)
            {
                xaxis.IntervalType = Syncfusion.SfChart.XForms.DateTimeIntervalType.Days;
                //xaxis.Interval = .5;
                xaxis.LabelStyle.LabelFormat = "M/d - h tt";

            }
            if (finish.DayOfYear - start.DayOfYear < 3)
            {
                xaxis.IntervalType = Syncfusion.SfChart.XForms.DateTimeIntervalType.Hours;
                //xaxis.Interval = 8;
                xaxis.LabelStyle.LabelFormat = "M/d - h tt";
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
