using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.SfChart.XForms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xapp2.Models;
using Xapp2.Models.ViewModel;

namespace Xapp2.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnalyticsPage : ContentPage
    {
        //Navigation open tracker
        bool Nav;
        DateTimeAxis test = new DateTimeAxis();
        //Interest Area Trackers
        string currentunit;
        string currentvessel;
        DateTime start = new DateTime();
        DateTime finish = new DateTime();

        //Analytics List References
        List<DateTimeRange> chartdata = new List<DateTimeRange> { new DateTimeRange(), new DateTimeRange() };
        List<AnalyticsLog> logs = new List<AnalyticsLog>();

        //pickerlist reference
        List<string> Companylist = new List<string>();

        //Company Selection Trackers
        bool[] CSeriesOn = new bool[] { false, false };
        string[] CSeriesSelect = new string[] { "Default", "NA" };
        public AnalyticsPage()
        {
            Nav = true;
            
            InitializeComponent();
            BindingContext = chartdata;
            SetUnitList();
        }

        private async void SetUnitList()
        {
            var unitlistall = await App.Database.GetUnits();
            var unitlist = unitlistall.Select(c => c.Name).ToList();
            unitpicker.ItemsSource = unitlist;

            if (Globals.init)
            {
                currentunit = Globals.unit;
                currentvessel = Globals.vessel;
                unitpicker.SelectedItem = currentunit;
                vesselpicker.SelectedItem = currentvessel;
            }
        }
        private async void SetVesselList()
        {
            var vessellistall = await App.Database.GetVessels();
            var vessellistfilter = vessellistall.Where(w => w.Unitname == currentunit);
            List<string> vessellist = vessellistfilter.Select(c => c.Name).ToList();

            vesselpicker.ItemsSource = vessellist;
            if (Globals.init)
            { vesselpicker.SelectedItem = currentvessel; }
        }
        private async void SetCompanyList()
        {
            Company1picker.IsEnabled = CSeriesOn[0]; Company2picker.IsEnabled = CSeriesOn[1];

            if (CSeriesOn[0])
            {
                Companylist.Clear();
                Companylist.Add("NA");
                List<string> tempcomp = chartdata[0].DateTimeData.Select(w => w.Company).ToList();
                Companylist.AddRange(tempcomp.Distinct().ToList());

                Company1picker.ItemsSource = new List<string> { "Default" , "Default Off" };
                Company1picker.SelectedItem = "Default";
                Company2picker.ItemsSource = Companylist;
            }
            else
            {

            }
            
        }
        async void OnUnitPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                currentunit = (string)picker.ItemsSource[selectedIndex];
                CSeriesOn[0] = false; CSeriesOn[1] = false; //disabble all company pickers
                SetVesselList();

                if (currentunit != Globals.unit)
                {
                    Globals.init = false; //Removing global item selection
                }
            }
        }
        async void OnVesselPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                currentvessel = (string)picker.ItemsSource[selectedIndex];
                await SetAnalyticsList();

                CSeriesOn[0] = true; CSeriesOn[1] = true; //enable 2x company pickers
                SetCompanyList();
            }
        }
        private async void OnCompanyPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                CSeriesSelect[0] = (string)Company1picker.SelectedItem;
                CSeriesSelect[1] = (string)Company2picker.SelectedItem;

                if (CSeriesOn[0] & chartdata[0].DateTimeData.Count > 0) // do not activate plot if vessel has not been selected and if no data exists
                {
                    await SetAnalyticsList();
                    
                }
            }
        }

        private async Task<int> SetAnalyticsList()
        {
            // Pull and filter all entry logs on specific location
            logs = await App.Database.GetAnalytics();
            logs = logs.Where(w => w.VesselName == currentvessel & w.UnitName == currentunit).ToList();
            logs.Sort((x, y) => DateTime.Compare(x.TimeLog, y.TimeLog));

            var workerlistall = await App.Database.GetWorkers();

            if (logs.Count > 0) //only generate chart data if logs exist
            {

                chartdata[0].DateTimeData.Clear();
                chartdata[1].DateTimeData.Clear();
                int secondseries = 1;

                if (CSeriesSelect[1] != "NA") { secondseries = 2; }

                for (int k = 0; k < secondseries; k++)
                {
                    int markers = 0; //tracks number of datapoint in series

                    if (logs.Count > 0)
                    {

                        for (int z = 0; z < logs.Count; z++)
                        {

                            LogModel temprange = new LogModel(); //New graphical point at different Y point
                            LogModel temprange2 = new LogModel();//Carry old Y value to new X value to create step effect
                            IEnumerable<Worker> properworker = workerlistall.Where(w => w.ReferenceNFC == logs[z].ReferenceNFC);

                            temprange.Date = logs[z].TimeLog;
                            temprange2.Date = logs[z].TimeLog;
                            temprange.Company = properworker.FirstOrDefault().Company;
                            temprange2.Company = temprange.Company;

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
                            if (k == 1 & temprange.Company == CSeriesSelect[1])
                            {
                                chartdata[k].DateTimeData.Add(temprange2);
                                chartdata[k].DateTimeData.Add(temprange);
                                markers++;
                            }
                        }
                    }
                }
                if (start.Year == 1)
                {
                    start = chartdata[0].DateTimeData[0].Date;
                    finish = chartdata[0].DateTimeData[2 * logs.Count - 1].Date;
                }
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
            int num;
            if (CSeriesSelect[1] == "NA" | CSeriesSelect[1] == null)
            {      num = 1; }
            else { num = 2; }

            //Reducing series size based on range selection
            for (int k=0; k < num; k++)
            {
                for (int i=0;i<chartdata[k].DateTimeData.Count; i++)
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
            sfchart.ItemsSource = chartdata2[2].DateTimeData;
            sfchart2.ItemsSource = chartdata2[2].DateTimeData;
            //Displaying series if selected
            if (CSeriesSelect[0] == "Default")
            {
                sfchart.ItemsSource = chartdata2[0].DateTimeData;
            }
            if (CSeriesSelect[1] != "NA") //only display secondary axis if a particular company is selected
            {
                sfchart2.ItemsSource = chartdata2[1].DateTimeData;
            }

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

        private async void RangeChangeClicked(object sender, EventArgs e)
        {
            //saving initial values in case of error
            DateTime refstart = start;
            DateTime reffinish = finish;

            //Pull range change quantity selection
            List<int> rangechange = new List<int> { Convert.ToInt32(comboBox1.SelectedItem) , Convert.ToInt32(comboBox2.SelectedItem), Convert.ToInt32(comboBox3.SelectedItem) };


            var rangereq = (Button)sender;
            string type = rangereq.AutomationId;    //1 = -hour, 2 = -day, 3 = -month, 4 = +hour, 5 = +day, 6 = +month
            string type2 = rangereq.StyleId;        //1 = range start, 2 = range end
            if (type2 == "1")
            {
                if (type == "1")
                { start = start.AddHours(-rangechange[0]); }
                if (type == "2")
                { start = start.AddDays(-rangechange[1]); }
                if (type == "3")
                { start = start.AddMonths(-rangechange[2]); }
                if (type == "4")
                { start = start.AddHours(rangechange[0]); }
                if (type == "5")
                { start = start.AddDays(rangechange[1]); }
                if (type == "6")
                { start = start.AddMonths(rangechange[2]); }
            }
            if (type2 == "2")
            {
                if (type == "1")
                { finish = finish.AddHours(-rangechange[0]); }
                if (type == "2")
                { finish = finish.AddDays(-rangechange[1]); }
                if (type == "3")
                { finish = finish.AddMonths(-rangechange[2]); }
                if (type == "4")
                { finish = finish.AddHours(rangechange[0]); }
                if (type == "5")
                { finish = finish.AddDays(rangechange[1]); }
                if (type == "6")
                { finish = finish.AddMonths(rangechange[2]); }
            }

            if (start > finish | finish < start)
            {
                start = refstart;
                finish = reffinish;
                await DisplayAlert("Range Error", "Invalid Range Selection", "Return to Entry");
            }

            if (CSeriesOn[0] & chartdata[0].DateTimeData.Count > 0) // do not activate plot if vessel has not been selected and if no data exists
            {
                SendToChart();
            }
        }

        private async void AutoEntryToggle(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(), false).ConfigureAwait(false);
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