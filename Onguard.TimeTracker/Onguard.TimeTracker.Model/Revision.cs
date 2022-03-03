using System;

namespace Onguard.TimeTracker.Model
{
    public class Revision
    {
        public enum StateEnum
        {
            ToDo,
            InProgress,
            Done
        }

        public StateEnum State { get; }
        public DateTime DateChanged { get; }
        public string ChangedBy { get; }

        public Revision(StateEnum state, DateTime dateChanged, string changedBy)
        {
            State = state;
            DateChanged = dateChanged;
            ChangedBy = changedBy;
        }
    }
}