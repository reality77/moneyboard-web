﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using dto.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using web.Models;
using web.Services;

namespace web.Controllers
{
    [Route("transactions")]
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
    }
}
