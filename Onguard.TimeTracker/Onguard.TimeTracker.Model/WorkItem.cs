using System;
using System.Collections.Generic;

namespace Onguard.TimeTracker.Model
{
    public class WorkItem
    {
        public enum WorkItemType
        {
            ProductBacklogItem,
            Defect
        }

        public int Id { get; }
        public string Title { get; }
        public WorkItemType Type { get; }
        public List<Task> Tasks { get; }
    }

}