using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Onguard.TimeTracker.BLL
{
    public class InputViewModelFactory
    {
        public InputViewModel Create(IFormCollection formCollection)
        {
            var project = formCollection["Input.SelectedProject"];
            var team = formCollection["Input.SelectedTeam"];
            var sprint = formCollection["Input.SelectedSprint"];
            var pbiTag = formCollection["Input.PbiTag"];
            var startDate = string.IsNullOrEmpty(formCollection["Input.SelectedStartDate"]) ? DateTime.MinValue : DateTime.Parse(formCollection["Input.SelectedStartDate"]);
            var endDate = string.IsNullOrEmpty(formCollection["Input.SelectedEndDate"]) ? DateTime.MinValue : DateTime.Parse(formCollection["Input.SelectedEndDate"]);

            string inputIncludeWorkItemTypePbi = formCollection["Input.IncludeWorkItemTypePbi"];
            string inputIncludeWorkItemTypeDefect = formCollection["Input.IncludeWorkItemTypeDefect"];
            var includeWorkItemTypePbi = string.IsNullOrEmpty(inputIncludeWorkItemTypePbi) || bool.Parse(inputIncludeWorkItemTypePbi.Split(',').First());
            var includeWorkItemTypeDefect = string.IsNullOrEmpty(inputIncludeWorkItemTypeDefect) || bool.Parse(inputIncludeWorkItemTypeDefect.Split(',').First());

            return new InputViewModel
            {
                SelectedProject = project,
                SelectedTeam = team,
                SelectedSprint = sprint,
                SelectedStartDate = startDate,
                SelectedEndDate = endDate,
                IncludeWorkItemTypePbi = includeWorkItemTypePbi,
                IncludeWorkItemTypeDefect = includeWorkItemTypeDefect,
                PbiTag = pbiTag
            };
        }
    }
}