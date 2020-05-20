using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Xapp2.Models.ViewModel
{
    public class UnitPieView
    {
        public ObservableCollection<UnitPieModel> piemodel { get; set; }

        public UnitPieView()
        {
            piemodel = new ObservableCollection<UnitPieModel>();
            pielabels = new List<string>();
        }
        public int pieselect { get; set; }

        public List<string> pielabels { get; set; }

    }
}
