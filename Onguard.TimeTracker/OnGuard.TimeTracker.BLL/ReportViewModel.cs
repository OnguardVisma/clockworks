using System;
using System.Collections.Generic;
using System.Text;

namespace Onguard.TimeTracker.BLL
{
    public class ReportViewModel
    {
        public PercentageViewModel PercentageView {  get; set;}
        public IEnumerable<ReportRowViewModel> ReportRowViews { get; set; }
    }
}
