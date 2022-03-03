using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Onguard.TimeTracker.BLL
{
    public class HomeViewModel
    {
        /// <summary>
        /// Project collection
        /// </summary>
        public IEnumerable<SelectListItem> Projects { get; set; }

        /// <summary>
        /// Input
        /// </summary>
        public InputViewModel Input { get; set; }

        public ReportViewModel Report { get; set; }

        /// <summary>
        /// Indicator if the view has enough data to generate a report with
        /// </summary>
        public bool ReadyToReport => Input.ReadyToReport;
    }
}