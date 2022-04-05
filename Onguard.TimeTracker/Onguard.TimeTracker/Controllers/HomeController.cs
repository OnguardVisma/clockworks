using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Onguard.TimeTracker.BLL;
using Onguard.TimeTracker.DAL;
using Onguard.TimeTracker.Models;

namespace Onguard.TimeTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HomeViewModelFactory _homeViewModelFactory;

        public HomeController(IVstsApi vstsApi)
        {
            _homeViewModelFactory = new HomeViewModelFactory(vstsApi);
        }

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var viewModel = await _homeViewModelFactory.Create();
            return View(viewModel);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject(IFormCollection formCollection)
        {
            var viewModel = await _homeViewModelFactory.CreateWithProject(formCollection);
            return View("Home", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDates(IFormCollection formCollection)
        {
            var viewModel = await _homeViewModelFactory.CreateWithDates(formCollection);
            return View("Home", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> GenerateReport(IFormCollection formCollection)
        {
            var action = formCollection["submit"];

            var viewModel = await _homeViewModelFactory.CreateWithInput(formCollection);

            return action.Equals("CSV") ? CsvHelper.ConvertReportToCsvFile(viewModel) : (ActionResult)View("Home", viewModel);
        }
    }
}