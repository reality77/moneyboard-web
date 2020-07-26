using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using web.Models;
using web.Services;

namespace web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiClient _api;

        public HomeController(ILogger<HomeController> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _api.GetAsync<IEnumerable<dto.Model.AccountBase>>("accounts");

            ViewBag.TotalBalance = accounts.Sum(a => a.Balance);

            return View(accounts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
