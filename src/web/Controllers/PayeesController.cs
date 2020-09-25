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
using web.Utils;

namespace web.Controllers
{
    [Route("payees")]
    [Authorize]
    public class PayeesController : Controller
    {
        private readonly ILogger<PayeesController> _logger;
        private readonly ApiClient _api;

        public PayeesController(ILogger<PayeesController> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        [Route("{key}")]
        public async Task<IActionResult> Details(string key)
        {
            var details = await _api.GetAsync<dto.Model.Tag>($"tags/payee/{key.ToCleanQuery()}");
            var transactions = await _api.GetAsync<IEnumerable<dto.Model.Transaction>>($"transactions/tag/payee/{key.ToCleanQuery()}");

            var request = new TagStatisticsRequest
            {
                Range = EDateRange.Months,
                DateStart = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, 1),
                DateEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month + 1, 1),
            };

            var stats = await _api.PostAsync<IEnumerable<dto.Model.TagStatisticsModel>>($"tags/payee/{key.ToCleanQuery()}/statistics", System.Text.Json.JsonSerializer.Serialize(request), "application/json");

            var statsOrdered = stats.OrderBy(s => s.Year).ThenBy(s => s.Month);

            var statsWithEmptyMonthsFilled = new List<dto.Model.TagStatisticsModel>();

            dto.Model.TagStatisticsModel lastStat = null;
            int minYear = 0;

            foreach(var stat in statsOrdered)
            {
                if(lastStat != null)
                {
                    int lastMonthIndex = ((lastStat.Year - minYear) + 1) * lastStat.Month.Value;
                    int currentMonthIndex = ((stat.Year - minYear) + 1) * stat.Month.Value;

                    for(int monthIndex = lastMonthIndex + 1; monthIndex < currentMonthIndex; monthIndex ++)
                        statsWithEmptyMonthsFilled.Add(new TagStatisticsModel { Year = minYear, Month = monthIndex, Day = null, Total = 0 });
                }
                else
                    minYear = stat.Year;

                statsWithEmptyMonthsFilled.Add(stat);

                lastStat = stat;
            }

            var model = new PayeeModel
            {
                Details = details,
                Transactions = transactions.OrderByDescending(t => t.UserDate).ThenByDescending(t => t.Date),
                Statistics = statsWithEmptyMonthsFilled,
            };

            return View(model);
        }
    }
}
