using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Onguard.TimeTracker.BLL;
using Onguard.TimeTracker.Models;

namespace Onguard.TimeTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HomeViewModelFactory _homeViewModelFactory;

        public HomeController()
        {
            _homeViewModelFactory = new HomeViewModelFactory();
        }

        [HttpGet]
        public IActionResult Home()
        {
            var viewModel = _homeViewModelFactory.Create();
            return View(viewModel);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
        
        [HttpPost]
        public ActionResult UpdateProject(IFormCollection formCollection)
        {
            var viewModel = _homeViewModelFactory.CreateWithProject(formCollection);
            return View("Home", viewModel);
        }

        [HttpPost]
        public ActionResult UpdateDates(IFormCollection formCollection)
        {
            var viewModel = _homeViewModelFactory.CreateWithDates(formCollection);
            return View("Home", viewModel);
        }

        [HttpPost]
        public ActionResult GenerateReport(IFormCollection formCollection)
        {
            var action = formCollection["submit"];

            if (action.Equals("CSV"))
            {
                var viewModelCsv = _homeViewModelFactory.CreateWithInput(formCollection);
                var csvFile = CsvHelper.ConvertReportToCsvFile(viewModelCsv);
                return csvFile;
            }

            var viewModelView = _homeViewModelFactory.CreateWithInput(formCollection);
            return View("Home", viewModelView);
        }
    }
}