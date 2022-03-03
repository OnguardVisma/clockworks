using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Onguard.TimeTracker.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Onguard.TimeTracker.BLL
{

    public class HomeViewModelFactory
    {
        private readonly InputViewModelFactory _inputViewModelFactory;
        private readonly VstsApi _vstsApi;

        public HomeViewModelFactory()
        {
            _vstsApi = new VstsApi(VstsApiConfiguration.Url, VstsApiConfiguration.Psa);
            _inputViewModelFactory = new InputViewModelFactory();
        }

        public HomeViewModel CreateWithProject(IFormCollection formCollection)
        {
            var inputViewModel = _inputViewModelFactory.Create(formCollection);

            var viewModel = new HomeViewModel
            {
                Input = _inputViewModelFactory.Create(formCollection),
                Projects = _vstsApi.GetProjects().Select(p => new SelectListItem() { Text = p.Name, Value = p.Name, Selected = p.Name == inputViewModel.SelectedProject }),
            };

            return viewModel;
        }

        public HomeViewModel Create()
        {
            var viewModel = new HomeViewModel
            {
                Input = new InputViewModel(),
                Projects = _vstsApi.GetProjects().Select(p => new SelectListItem() { Text = p.Name, Value = p.Name })
            };

            return viewModel;
        }

        public HomeViewModel CreateWithDates(IFormCollection formCollection)
        {
            var inputViewModel = _inputViewModelFactory.Create(formCollection);

            var viewModel = new HomeViewModel
            {
                Input = inputViewModel,
                Projects = _vstsApi.GetProjects().Select(p => new SelectListItem { Text = p.Name, Value = p.Name })
            };

            return viewModel;
        }

        public HomeViewModel CreateWithInput(IFormCollection formCollection)
        {
            var inputViewModel = _inputViewModelFactory.Create(formCollection);

            if (!string.IsNullOrEmpty(inputViewModel.SelectedProject) && !string.IsNullOrEmpty(inputViewModel.SelectedTeam) && !string.IsNullOrEmpty(inputViewModel.SelectedSprint))
            {
                var projects = _vstsApi.GetProjects().Select(p => new SelectListItem { Text = p.Name, Value = p.Name });
                var report = ConstructViewModel(inputViewModel, _vstsApi.GetReport(inputViewModel.SelectedProject, inputViewModel.SelectedTeam, inputViewModel.SelectedSprint));

                // Generate report based on sprint
                return new HomeViewModel
                {
                    Input = inputViewModel,
                    Projects = projects,
                    Report = report
                };
            }

            // Generate report based on dates
            return new HomeViewModel
            {
                Input = inputViewModel,
                Projects = _vstsApi.GetProjects().Select(p => new SelectListItem { Text = p.Name, Value = p.Name }),

                Report = ConstructViewModel(inputViewModel, _vstsApi.GetReport(inputViewModel.SelectedProject,
                    inputViewModel.SelectedTeam, inputViewModel.SelectedStartDate, inputViewModel.SelectedEndDate,
                    inputViewModel.PbiTag, inputViewModel.IncludeWorkItemTypePbi, inputViewModel.IncludeWorkItemTypeDefect))
            };
        }

        private ReportViewModel ConstructViewModel(InputViewModel inputViewModel, IEnumerable<WorkItem> workItems)
        {
            // Create WorkItemViewModel
            var workItemsViewModel = new List<ReportRowViewModel>();

            // Collect the Unique PBI id's
            var enumerable = workItems as WorkItem[] ?? workItems.ToArray();
            var workItemRelations = (
                from workItem in enumerable.ToArray()
                where enumerable.Any(a => a.Relations.Any(r => r.Rel == "System.LinkTypes.Hierarchy-Reverse"))
                select workItem.Relations.FirstOrDefault())?.ToList();

            var unqiqueProductBacklogItems = workItemRelations.Select(a => a
                .Url.Split('/')
                .LastOrDefault())
            .Select(int.Parse)
            .Distinct()
            .ToList();

            var percentageViewModel = new PercentageViewModel()
            {
                DevelopmentHours = TimeSpan.Zero,
                DevelopmentPercentage = 0,
                MaintenanceHours = TimeSpan.Zero,
                MaintenancePercentage = 0
            };

            // Get the PBI's
            var productBacklogItems = _vstsApi.GetWorkItems(unqiqueProductBacklogItems);

            foreach (var workItem in enumerable)
            {
                // Collect the revisions of the work item
                if (workItem.Id != null)
                {
                    const string ToDo = "To Do";
                    const string InProgress = "In Progress";
                    const string Done = "Done";

                    var workItemRevisions = _vstsApi.GetWorkItemRevisions(workItem.Id.Value);

                    var firstDate = DateTime.Parse(workItemRevisions.Values[0].Fields.WorkItemChangedDate);
                    var lastDate = DateTime.Parse(workItemRevisions.Values[workItemRevisions.Count - 1].Fields.WorkItemChangedDate);

                    var isBusy = false;
                    var previousState = "To Do";
                    var pbiId = int.Parse((string)(workItem.Relations.FirstOrDefault(r => r.Rel == "System.LinkTypes.Hierarchy-Reverse")?.Url.Split('/').LastOrDefault() ?? throw new InvalidOperationException()));
                    var assignee = "";
                    var prevAssignee = "";

                    // Make report line per day
                    foreach (DateTime day in EachDay(firstDate.Date, lastDate.Date))
                    {
                        if (HourCalculator.IsWorkday(day))
                        {
                            var hoursWorked = TimeSpan.Zero;
                            var workStartTime = HourCalculator.StartOfBusiness(day);
                            var workEndTime = HourCalculator.CloseOfBusiness(day);

                            var revs = workItemRevisions.Values.Where(x => DateTime.Parse(x.Fields.WorkItemChangedDate).Date == day.Date);
                            foreach (var rev in revs)
                            {
                                assignee = rev.Fields.WorkItemAssignedTo?.DisplayName ?? rev.Fields.WorkItemChangedBy.DisplayName;
                                if (assignee != prevAssignee)
                                {
                                    if (prevAssignee != "" && hoursWorked > TimeSpan.Zero)
                                    {
                                        var reportRowViewModel = new ReportRowViewModel
                                        {
                                            Pbi = productBacklogItems.Single(pbi => pbi.Id == pbiId),
                                            PbiUrl = VstsApiConfiguration.Url + $"/{inputViewModel.SelectedProject}/_workitems/edit/{productBacklogItems.Single(pbi => pbi.Id == pbiId).Id}",
                                            VstsWorkItem = workItem,
                                            TaskId = workItem.Id.Value,
                                            TaskUrl = VstsApiConfiguration.Url + $"/{inputViewModel.SelectedProject}/_workitems/edit/{workItem.Id.Value}",
                                            StartDate = workStartTime,
                                            DoneDate = workEndTime,
                                            TotalWorkHours = TimeSpan.FromHours(hoursWorked.TotalHours),
                                            DoneBy = prevAssignee,
                                        };
                                        var wi = reportRowViewModel.Pbi;
                                        var t = wi.Fields["System.WorkItemType"];
                                        if (t.ToString() == "Defect") { reportRowViewModel.DM = "Mnt"; }
                                        if (t.ToString() == "Product Backlog Item") { reportRowViewModel.DM = "Dev"; }

                                        workItemsViewModel.Add(reportRowViewModel);
                                        UpdatePercentage(percentageViewModel, hoursWorked, reportRowViewModel);

                                        hoursWorked = TimeSpan.Zero;
                                    }
                                    prevAssignee = assignee;
                                }

                                if (rev.Fields.WorkItemState == ToDo && previousState != ToDo)
                                {
                                    if (isBusy)
                                    {
                                        isBusy = false;
                                        workEndTime = DateTime.Parse(rev.Fields.WorkItemChangedDate);
                                        hoursWorked += (workEndTime - workStartTime);
                                        previousState = ToDo;
                                    }
                                }

                                if (rev.Fields.WorkItemState == InProgress && previousState != InProgress)
                                {
                                    isBusy = true;
                                    workStartTime = DateTime.Parse(rev.Fields.WorkItemChangedDate);
                                    previousState = InProgress;
                                }

                                if (rev.Fields.WorkItemState == Done && previousState != Done)
                                {
                                    if (isBusy)
                                    {
                                        isBusy = false;
                                        workEndTime = DateTime.Parse(rev.Fields.WorkItemChangedDate);
                                        hoursWorked += (workEndTime - workStartTime);
                                        previousState = Done;
                                    }
                                }
                            }

                            if (isBusy)
                            {
                                hoursWorked += (workEndTime - workStartTime);
                            }

                            if (hoursWorked > TimeSpan.Zero)
                            {
                                var reportRowViewModel = new ReportRowViewModel
                                {
                                    Pbi = productBacklogItems.Single(pbi => pbi.Id == pbiId),
                                    PbiUrl = VstsApiConfiguration.Url + $"/{inputViewModel.SelectedProject}/_workitems/edit/{productBacklogItems.Single(pbi => pbi.Id == pbiId).Id}",
                                    VstsWorkItem = workItem,
                                    TaskId = workItem.Id.Value,
                                    TaskUrl = VstsApiConfiguration.Url + $"/{inputViewModel.SelectedProject}/_workitems/edit/{workItem.Id.Value}",
                                    StartDate = workStartTime,
                                    DoneDate = workEndTime,
                                    TotalWorkHours = TimeSpan.FromHours(hoursWorked.TotalHours),
                                    DoneBy = assignee
                                };
                                var wi = reportRowViewModel.Pbi;
                                var t = wi.Fields["System.WorkItemType"];
                                if (t.ToString() == "Defect") { reportRowViewModel.DM = "Mnt"; }
                                if (t.ToString() == "Product Backlog Item") { reportRowViewModel.DM = "Dev"; }

                                workItemsViewModel.Add(reportRowViewModel);

                                UpdatePercentage(percentageViewModel, hoursWorked, reportRowViewModel);
                            }
                        }
                    }
                }
            }


            // Find overlapping tasks
            foreach (var workItemViewModel in workItemsViewModel)
            {
                var overlappingTasks = workItemsViewModel
                    .Where(vm => vm.DoneBy.Equals(workItemViewModel.DoneBy))
                    .Where(vm => vm.IntersectsWith(workItemViewModel) && vm.TaskId != workItemViewModel.TaskId).ToList();
                workItemViewModel.OverlappingTasks = overlappingTasks.Distinct();
            }

            return new ReportViewModel()
            {
                ReportRowViews = workItemsViewModel,
                PercentageView = percentageViewModel
            };
        }

        private static void UpdatePercentage(PercentageViewModel percentageViewModel, TimeSpan hoursWorked, ReportRowViewModel reportRowViewModel)
        {
            var wi = reportRowViewModel.Pbi;
            var t = wi.Fields["System.WorkItemType"];
            if (t.ToString() == "Product Backlog Item")
            {
                percentageViewModel.DevelopmentHours += TimeSpan.FromHours(hoursWorked.TotalHours);
            }
            if (t.ToString() == "Defect")
            {
                percentageViewModel.MaintenanceHours += TimeSpan.FromHours(hoursWorked.TotalHours);
            }
            if (percentageViewModel.MaintenanceHours + percentageViewModel.DevelopmentHours > TimeSpan.Zero)
            {
                percentageViewModel.MaintenancePercentage = (100 * percentageViewModel.MaintenanceHours) / (percentageViewModel.MaintenanceHours + percentageViewModel.DevelopmentHours);
                percentageViewModel.DevelopmentPercentage = (100 * percentageViewModel.DevelopmentHours) / (percentageViewModel.MaintenanceHours + percentageViewModel.DevelopmentHours);
            }
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

    }
}