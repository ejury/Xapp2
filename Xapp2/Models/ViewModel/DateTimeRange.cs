using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Xapp2.Models.ViewModel
{
    public class DateTimeRange
    {
		public ObservableCollection<LogModel> DateTimeData { get; set; }

		public DateTimeRange()
		{
			DateTimeData = new ObservableCollection<LogModel>();

		}

		public DateTime Minimum { get; set; }
		public DateTime Maximum { get; set; }
	}

	public class LogModel
	{
		public DateTime Date { get; set; }

		public double Value { get; set; }

		public string Company { get; set; }

	}

	//public class DateTimeAxis : RangeAxisBase, Xamarin.Forms.IElementController
	//{ }
	//public class DateTimeAxisLabel : ChartAxisLabel 
}
