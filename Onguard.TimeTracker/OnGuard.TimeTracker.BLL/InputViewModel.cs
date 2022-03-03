using System;
using System.ComponentModel.DataAnnotations;

namespace Onguard.TimeTracker.BLL
{
    public class InputViewModel
    {
        /// <summary>
        /// Selected Project
        /// </summary>
        public string SelectedProject { get; set; }

        /// <summary>
        /// Selected Team
        /// </summary>
        public string SelectedTeam { get; set; }

        /// <summary>
        /// Selected Sprint
        /// </summary>
        public string SelectedSprint { get; set; }

        /// <summary>
        /// SelectedStartDate
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime SelectedStartDate { get; set; }

        /// <summary>
        /// SelectedEndDate
        /// </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime SelectedEndDate { get; set; }

        /// <summary>
        /// Include WorkItemType PBI
        /// </summary>
        [Display(Name = "Pbi")]
        public bool IncludeWorkItemTypePbi { get; set; }

        /// <summary>
        /// Include WorkItemType Defect
        /// </summary>
        [Display(Name = "Defect")]
        public bool IncludeWorkItemTypeDefect { get; set; }

        /// <summary>
        /// PBI Tag
        /// </summary>
        [Display(Name = "PBI Tag")]
        public string PbiTag { get; set; }

        /// <summary>
        /// Indicator if the view has enough data to generate a report with
        /// </summary>
        public bool ReadyToReport
        {
            get
            {
                if (!string.IsNullOrEmpty(SelectedProject) && !string.IsNullOrEmpty(SelectedSprint))
                    return true;
                return !string.IsNullOrEmpty(SelectedProject) && SelectedStartDate != DateTime.MinValue && SelectedEndDate != DateTime.MinValue;
            }
        }
    }
}