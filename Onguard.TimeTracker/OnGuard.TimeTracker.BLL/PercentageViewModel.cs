using System;
using System.Runtime.Serialization;


namespace Onguard.TimeTracker.BLL
{
    [DataContract]
    public class PercentageViewModel
    {
        [DataMember]
        public TimeSpan MaintenanceHours { get; set; }
        [DataMember]
        public double MaintenancePercentage { get; set; }
        [DataMember]
        public TimeSpan DevelopmentHours { get; set; }
        [DataMember]
        public double DevelopmentPercentage { get; set; }
    }
}
