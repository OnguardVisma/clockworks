using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace Onguard.TimeTracker.DAL
{
    public class VstsApi : IVstsApi
    {
        /// <summary>
        /// Url of VSTS
        /// </summary>
        private string Url { get; }

        /// <summary>
        /// Required identifier that can be created through VSTS
        /// </summary>
        private string PersonalAccessToken { get; }

        /// <summary>
        /// ApiVersion
        /// </summary>
        private string ApiVersion { get; }

        /// <summary>
        /// Credentials that get generated for your PersonalAccessToken
        /// </summary>
        private readonly VssCredentials _vssCredentials;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url">VSTS url</param>
        /// <param name="personalAccessToken">Personal Access Token</param>
        public VstsApi(string url, string personalAccessToken)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrEmpty(personalAccessToken)) throw new ArgumentNullException(nameof(url));

            Url = url;
            PersonalAccessToken = personalAccessToken;
            ApiVersion = "";
            _vssCredentials = new VssBasicCredential("", PersonalAccessToken);
        }

        /// <inheritdoc />
        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns>Collection of project names</returns>
        public IEnumerable<ProjectModel> GetProjects()
        {
            var result = SendApiUrlRequest("_apis/projects?stateFilter=All" + ApiVersion).Content
                .ReadAsAsync<VstsListResponse.List>().Result;

            return result.Values.Length > 0 ? result.Values.Select(r => new ProjectModel(r.Name, r.Url)) : null;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Get work items for given sprint within a project
        /// </summary>
        /// <returns>Collection of work items</returns>
        public IEnumerable<WorkItem> GetReport(string project, string team, string sprint)
        {
            if (string.IsNullOrEmpty(project)) throw new ArgumentNullException(nameof(project));
            if (string.IsNullOrEmpty(sprint)) throw new ArgumentNullException(nameof(sprint));

            // Consruct query
            var wiql = new Wiql()
            {
                Query = "Select * " +
                        "From WorkItems " +
                        "Where [System.TeamProject] = '" + project + "' " +
                        "And [System.IterationPath] = '" + project + "\\" + sprint + "' " +
                        //"And [System.AreaPath] = '" + project + "\\" + team + "' " +
                        "And [System.WorkItemType] = 'Task' " +
                        "And [System.State] = 'Done' " +
                        "Order By [Id] Asc"
            };

            return GetWorkItems(wiql);
        }

        /// <inheritdoc />
        /// <summary>
        /// Get work items between given dates within a project
        /// </summary>
        /// <returns>Collection of work items</returns>
        public IEnumerable<WorkItem> GetReport(string project, string team, DateTime startDate, DateTime endDate,
            string tag, bool includePbis, bool includeDefects)
        {
            if (string.IsNullOrEmpty(project)) throw new ArgumentNullException(nameof(project));
            if (startDate == DateTime.MinValue) throw new ArgumentNullException(nameof(startDate));
            if (endDate == DateTime.MinValue) throw new ArgumentNullException(nameof(endDate));

            if (endDate < startDate) throw new ArgumentException("enddate can not be earlier then startdate");

            var queries = new List<Wiql>();

            foreach (DateTime Day in EachDay(startDate, endDate))
            {
                var wiql = new Wiql();
                    var pbiSelect = "";
                    if (includeDefects)
                    {
                        pbiSelect = "AND Target.[System.WorkItemType] = 'Defect' ";
                    }
                    if (includePbis)
                    {
                        pbiSelect = "AND Target.[System.WorkItemType] = 'Product Backlog Item' ";
                    }
                    if (includePbis && includeDefects)
                    {
                        pbiSelect = "AND (Target.[System.WorkItemType] = 'Product Backlog Item' OR Target.[System.WorkItemType] = 'Defect')";
                    }

                    var tagSelect = "";
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tagSelect = "AND Target.[System.Tags] contains '" + tag + "' "; }
                    wiql.Query = "SELECT [System.Id] " +
                                 "FROM WorkItemLinks " +
                                 "WHERE (Source.[System.TeamProject] = '" + project + "' " +
                                 "AND Source.[System.WorkItemType] = 'Task' " +
                                 "AND Source.[System.State] = 'Done' " +
                                 "AND Source.[Microsoft.VSTS.Common.ClosedDate] = '"
                                 + Day.ToString("yyyy-MM-ddTHH:mm:ss.ffffff") + "') " +
                                 "AND ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Reverse') " +
                                 "AND (Target.[System.TeamProject] = '" + project + "' " +
                                 pbiSelect +
                                 tagSelect +
                                 ") order by [System.Id] mode (MustContain)";
                

                queries.Add(wiql);
            }
            // Consruct query

            var result = new List<WorkItem>();

            foreach (var wiql in queries)
            {
                result.AddRange(GetWorkItems(wiql));
            }

            return result;
        }

        /// <summary>
        /// Internal method for sending http request
        /// </summary>
        /// <param name="url">url</param>
        /// <returns>HttpResponseMessage</returns>
        private HttpResponseMessage SendApiUrlRequest(string url)
        {
            //Prompt user for credential
            //VssConnection connection = new VssConnection(new Uri(url), new VssClientCredentials());

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{PersonalAccessToken}"));
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                return client.GetAsync(url).Result;
            }
        }

        /// <summary>
        /// Internal method for collection WorkItems based on a Wiql query
        /// </summary>
        /// <param name="wiql"></param>
        /// <returns></returns>
        private IEnumerable<WorkItem> GetWorkItems(Wiql wiql)
        {
            var result = new List<WorkItem>();
            
            using (var workItemTrackingHttpClient = new WorkItemTrackingHttpClient(new Uri(Url), _vssCredentials))
            {
                try
                {
                    var workItemQueryResult = workItemTrackingHttpClient.QueryByWiqlAsync(wiql).Result;
                    var workItems = new List<int>();

                    if (workItemQueryResult.WorkItemRelations != null)
                    {
                        workItems = (from workItemLink in workItemQueryResult.WorkItemRelations
                                where workItemLink.Source == null
                                select workItemLink.Target.Id)
                            .ToList();
                    }
                    else
                    {
                        workItems.AddRange(workItemQueryResult.WorkItems
                            .Select(workItemReference => workItemReference.Id));
                    }

                    ;

                    if (workItems.Any())
                    {
                        // Collect WorkItems
                        result = workItemTrackingHttpClient
                            .GetWorkItemsAsync(workItems, null, null, WorkItemExpand.Relations, null, null,
                                CancellationToken.None).Result;

                    }
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    message += "";
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<WorkItem> GetWorkItems(IEnumerable<int> ids)
        {
            var workItems = new List<WorkItem>();
            using (var workItemTrackingHttpClient = new WorkItemTrackingHttpClient(new Uri(Url), _vssCredentials))
            {
                var enumerableWOrkitemIds = ids.ToList();
                foreach (var workItemId in enumerableWOrkitemIds)
                {
                    WorkItem workItem = null;
                    try
                    {
                        workItem = workItemTrackingHttpClient.GetWorkItemsAsync(new[] {workItemId}, null, null,
                            WorkItemExpand.Relations).Result.FirstOrDefault();
                    }
                    finally
                    {
                        workItems.Add(workItem);
                    }
                }
            }

            return workItems;
        }

        /// <summary>
        /// Calls VSTS api to collect all revision of a given workitem
        /// </summary>
        /// <param name="workItemId"></param>
        /// <returns>WorkItems</returns>
        public ListofRevisionsReponse.Revisions GetWorkItemRevisions(int workItemId)
        {
            var response = SendApiUrlRequest($"_apis/wit/workitems/{workItemId}/revisions?$expand=relations" + ApiVersion);

            return response.IsSuccessStatusCode
                ? response.Content.ReadAsAsync<ListofRevisionsReponse.Revisions>().Result
                : new ListofRevisionsReponse.Revisions();
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}