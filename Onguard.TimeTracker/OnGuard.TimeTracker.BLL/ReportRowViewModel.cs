using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Onguard.TimeTracker.BLL
{
    [DataContract]
    public class ReportRowViewModel : IEqualityComparer<ReportRowViewModel>
    {
        [DataMember]
        public WorkItem Pbi { get; set; }

        public string DM { get; set; }

        public string PbiUrl { get; set; }

        public int TaskId { get; set; }

        public string TaskUrl { get; set; }

        public string DoneBy { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DoneDate { get; set; }

        public TimeSpan TotalWorkHours { get; set; }

        public WorkItem VstsWorkItem { get; set; }

        public IEnumerable<ReportRowViewModel> OverlappingTasks { get; set; }

        public bool Equals([AllowNull] ReportRowViewModel x, [AllowNull] ReportRowViewModel y)
        {
            return (x.DoneBy == DoneBy && x.TaskId == TaskId);
        }

        public int GetHashCode([DisallowNull] ReportRowViewModel obj)
        {
            return (DoneBy.Length + TaskId + DoneDate.Ticks).GetHashCode();
        }

        public bool IntersectsWith(ReportRowViewModel reportRowViewModel)
        {
            var intersecting = false || StartDate >= reportRowViewModel.StartDate && StartDate <= reportRowViewModel.DoneDate || DoneDate >= reportRowViewModel.StartDate && DoneDate <= reportRowViewModel.DoneDate;

            return intersecting;
        }
    }
}