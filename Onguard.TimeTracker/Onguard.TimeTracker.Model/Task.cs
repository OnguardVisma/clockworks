using System.Collections.Generic;

namespace Onguard.TimeTracker.Model
{
    public class Task
    {
        public int Id { get; }
        public string Title { get; }
        public List<Revision> Revisions { get; }
    }
}