using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Xapp2.Models.ViewModel
{
    class WorkerDoughnutView
    {
        public ObservableCollection<WorkerDoughnutModel> doughnutmodel { get; set; }

        public WorkerDoughnutView()
        {
            doughnutmodel = new ObservableCollection<WorkerDoughnutModel>();
            doughnutlabels = new List<string>();
        }
        public int doughnutselect { get; set; }

        public List<string> doughnutlabels { get; set; }
    }
}
