using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dto.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using web.Models;
using web.Services;
using web.Utils;

namespace web.Controllers
{
    public class TagControllerBase : Controller
    {
        protected readonly ILogger<TagControllerBase> _logger;
        protected readonly ApiClient _api;

        public TagControllerBase(ILogger<TagControllerBase> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        protected async Task<IActionResult> DetailsInternalAsync<T>(string tagType, string tag, string viewName = "TagDetails") where T : TagModel, new()
        {
            var details = await _api.GetAsync<dto.Model.Tag>($"tags/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}");
            var transactions = await _api.GetAsync<IEnumerable<dto.Model.Transaction>>($"transactions/tag/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}");

            var request = new TagStatisticsRequest
            {
                Range = EDateRange.Months,
                DateStart = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, 1).AddMonths(1),
                DateEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddMilliseconds(-1),
            };

            var stats = await _api.PostAsync<IEnumerable<dto.Model.TagStatisticsModel>>($"tags/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}/statistics", System.Text.Json.JsonSerializer.Serialize(request), "application/json");

            var dicStats = stats.ToDictionary(s => (Year: s.Year, Month: s.Month.Value));

            var statsWithEmptyMonthsFilled = new List<dto.Model.TagStatisticsModel>();

            for(var date = request.DateStart.Value; date <= request.DateEnd.Value; date = date.AddMonths(1))
            {
                if(dicStats.TryGetValue((date.Year, date.Month), out TagStatisticsModel stat))
                {
                    statsWithEmptyMonthsFilled.Add(stat);
                }
                else
                {
                    statsWithEmptyMonthsFilled.Add(new TagStatisticsModel 
                    { 
                        Year = date.Year, 
                        Month = date.Month, 
                        Day = null, 
                        Total = 0 }
                    );
                }    
            }

            var model = new T
            {
                Details = details,
                Transactions = transactions.OrderByDescending(t => t.UserDate).ThenByDescending(t => t.Date),
                Statistics = statsWithEmptyMonthsFilled,
            };

            return View(viewName, model);
        }
    }
}