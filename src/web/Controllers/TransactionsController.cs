using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using dto.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using web.Models;
using web.Services;

namespace web.Controllers
{
    [Route("transactions")]
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly ApiClient _api;

        public TransactionsController(ILogger<TransactionsController> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        [Route("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var details = await _api.GetAsync<dto.Model.ImportedTransaction>($"transactions/{id}");

            return View(details);
        }

        [ValidateAntiForgeryToken]
        [Route("{id}")]
        [HttpPost]
        public async Task<IActionResult> PostDetails(int id, [Bind("Caption,Comment,Date,UserDate")] ImportedTransaction transaction)
        {
            var trx = new TransactionEditRequest
            {
                Caption = transaction.Caption,
                Comment = transaction.Comment,
                Date = transaction.Date,
                UserDate = transaction.UserDate,
            };

            await _api.PutAsync($"transactions/{id}", trx);

            var details = await _api.GetAsync<dto.Model.ImportedTransaction>($"transactions/{id}");

            return View("Details", details);
        }
    }
}
