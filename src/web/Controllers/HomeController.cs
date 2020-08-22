using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if(this.HttpContext.User.Identity.IsAuthenticated)
            {
                var accounts = await _api.GetAsync<IEnumerable<dto.Model.AccountBase>>("accounts");
                var imports = await _api.GetAsync<IEnumerable<dto.Model.ImportedFile>>("import");

                ViewBag.TotalBalance = accounts.Sum(a => a.Balance);
                ViewBag.Updated = imports.Max(i => i.ImportDate);

                return View(accounts);
            }
            else
                return View();
        }

        [Route("~/login")]
        [Authorize]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
