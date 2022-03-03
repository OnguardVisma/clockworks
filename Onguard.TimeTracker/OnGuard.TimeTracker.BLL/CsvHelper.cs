using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Services.WebApi;

namespace Onguard.TimeTracker.BLL
{
    public static class CsvHelper
    {
        /// <summary>
        /// Converts a viewmodel to Csv string
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static FileContentResult ConvertReportToCsvFile(HomeViewModel viewModel)
        {
            var flattenedReportRows = viewModel.Report.ReportRowViews.Select(r => new FlatReportRowViewModel()
            {
                Pbi = r.Pbi.Id.ToString(),
                Title = r.Pbi.Fields["System.Title"].ToString(),
                TaskId = r.TaskId.ToString(),
                ChangedBy = r.DoneBy,
                StartDate = r.StartDate.ToString("yyyy/MM/dd HH:mm"),
                DoneDate = r.DoneDate.ToString("yyyy/MM/dd HH:mm"),
                TotalWorkHours = $"{r.TotalWorkHours:0.00}"
            }).ToList();

            ServiceStack.Text.CsvConfig.ItemSeperatorString = ";";
            var csvString = ServiceStack.Text.CsvSerializer.SerializeToCsv(flattenedReportRows);

            var result = new FileContentResult(Encoding.UTF8.GetBytes(csvString), "text/csv")
            {
                FileDownloadName = "OnguardReport_" + viewModel.Input.SelectedProject + ".csv"
            };

            return result;
        }
    }
}