using Newtonsoft.Json;

namespace Onguard.TimeTracker.DAL
{
    internal class VstsListResponse
    {
        /// <summary>
        /// List
        /// </summary>
        public class List
        {
            /// <summary>
            /// Count of items found by query
            /// </summary>
            [JsonProperty(PropertyName = "count")]
            public int Count { get; set; }

            /// <summary>
            /// Collection with values
            /// </summary>
            [JsonProperty(PropertyName = "value")]
            public Value[] Values { get; set; }
        }

        /// <summary>
        /// Value
        /// </summary>
        public class Value
        {
            /// <summary>
            /// Name of value
            /// </summary>
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            /// <summary>
            /// Corresponding url to VSTS item
            /// </summary>
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; }
        }
    }
}