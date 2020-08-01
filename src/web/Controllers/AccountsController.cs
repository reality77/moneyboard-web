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
    [Route("accounts")]
    public class AccountsController : Controller
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly ApiClient _api;

        public AccountsController(ILogger<AccountsController> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }


        [Route("{id}/transactions")]
        public async Task<IActionResult> Transactions(int id, int pageId = 0, int itemsPerPage = 200)
        {
            var details = await _api.GetAsync<dto.Model.AccountDetails>($"accounts/{id}");
            var transactions = await _api.GetAsync<IEnumerable<dto.Model.TransactionWithBalance>>($"accounts/{id}/transactions?pageId={pageId}&itemsPerPage={itemsPerPage}");

            var model = new AccountTransactionsModel
            {
                Details = details,
                Transactions = transactions,
            };

            return View(model);
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
