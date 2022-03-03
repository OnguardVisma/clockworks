using Newtonsoft.Json;

namespace Onguard.TimeTracker.DAL
{
    public class ListofRevisionsReponse
    {
        public class Revisions
        {
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            [JsonProperty(PropertyName = "value")]
            public Value[] Values { get; set; }
        }

        public class Value
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "rev")]
            public string Rev { get; set; }

            [JsonProperty(PropertyName = "fields")]
            public Field Fields { get; set; }
        }

        public class Field
        {
            [JsonProperty(PropertyName = "System.WorkItemType")]
            public string WorkItemType { get; set; }

            [JsonProperty(PropertyName = "System.State")]
            public string WorkItemState { get; set; }

            [JsonProperty(PropertyName = "System.ChangedDate")]
            public string WorkItemChangedDate { get; set; }

            [JsonProperty(PropertyName = "System.ChangedBy")]
            public Microsoft.VisualStudio.Services.WebApi.IdentityRef WorkItemChangedBy { get; set; }

            [JsonProperty(PropertyName = "System.AssignedTo")]
            public Microsoft.VisualStudio.Services.WebApi.IdentityRef WorkItemAssignedTo { get; set; }
        }
    }
}