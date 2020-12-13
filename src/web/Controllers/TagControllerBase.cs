using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dto.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        protected async Task<IActionResult> ListInternalAsync(string tagType, string viewName = "TagList")
        {
            var topleveltags = await _api.GetAsync<IEnumerable<dto.Model.Tag>>($"tagtypes/{tagType.ToCleanQuery()}/topleveltags");

            var list = await _api.GetAsync<IEnumerable<dto.Model.Tag>>($"tags/{tagType.ToCleanQuery()}");

            var request = new TagStatisticsRequest
            {
                Range = EDateRange.Range,
                DateStart = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, 1).AddMonths(1),
                DateEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddMilliseconds(-1),
                IncludeSubTags = true,
                ReturnSubTags = false
            };

            var chart = new ChartModel();

            int colorId = 0;

            foreach(var top in topleveltags)
            {
                var stats = await _api.PostAsync<IEnumerable<dto.Model.TagStatisticsModel>>($"tags/{tagType.ToCleanQuery()}/{top.Key.ToCleanQuery()}/statistics", System.Text.Json.JsonSerializer.Serialize(request), "application/json");

                var total = stats.SingleOrDefault()?.Total;

                if(total > 0)
                    continue;

                chart.Labels.Add(top.Caption ?? top.Key);
                chart.Series.Add(new ChartSerieModel
                {
                    BackgroundColor = ChartModel.DefaultColors[colorId++],
                    Values =  { total ?? 0m },
                });

                if(colorId == ChartModel.DefaultColors.Count())
                    colorId = 0;
            }

            return View(viewName, new TagListModel
            {
                TagType = tagType,
                Tags = list.OrderBy(t => t.Key),
                Chart = chart,
            });
        }

        protected async Task<IActionResult> DetailsInternalAsync<T>(string tagType, string tag, string viewName = "TagDetails") where T : TagModel, new()
        {
            var details = await _api.GetAsync<dto.Model.TagDetails>($"tags/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}");
            var transactions = await _api.GetAsync<IEnumerable<dto.Model.Transaction>>($"transactions/tag/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}");

            var request = new TagStatisticsRequest
            {
                Range = EDateRange.Months,
                DateStart = new DateTime(DateTime.Today.Year - 1, DateTime.Today.Month, 1).AddMonths(1),
                DateEnd = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddMilliseconds(-1),
                IncludeSubTags = true,
                ReturnSubTags = true,
            };

            var stats = await _api.PostAsync<IEnumerable<dto.Model.TagStatisticsModel>>($"tags/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}/statistics", System.Text.Json.JsonSerializer.Serialize(request), "application/json");

            var dicStats = stats.ToDictionary(s => (Year: s.Year, Month: s.Month.Value));

            var statsWithEmptyMonthsFilled = new List<dto.Model.TagStatisticsModel>();

            // --- Initialisation des données du tableau
            var chart = new ChartModel();
            var dicSeries = new Dictionary<string, ChartSerieModel>();

            var serie = new ChartSerieModel { Id = "total", Label = "Total", Type = "line" };
            dicSeries.Add(serie.Id, serie);
            chart.Series.Add(serie);

            serie = new ChartSerieModel { Id = "average", Label = "Moyenne", Type = "line" };
            dicSeries.Add(serie.Id, serie);
            chart.Series.Add(serie);

            serie = new ChartSerieModel { Id = $"tag:{details.Key}", Label = details.Caption ?? details.Key, Group = "main", BackgroundColor = ChartModel.DefaultColors[0] };
            dicSeries.Add(serie.Id, serie);
            chart.Series.Add(serie);

            var first = stats.FirstOrDefault();
            if(first != null)
            {
                int colorId = 1;

                foreach(var subTagTotal in first.SubTagTotals)
                {
                    serie = new ChartSerieModel { Id = $"tag:{subTagTotal.Tag.Key}", Label = subTagTotal.Tag.Caption ?? subTagTotal.Tag.Key, Group = "main", BackgroundColor = ChartModel.DefaultColors[colorId++] };
                    dicSeries.Add(serie.Id, serie);
                    chart.Series.Add(serie);
                }
            }

            var average = stats.Average(s => s.Total);
            
            // --- Initialisation des données du tableau
            for(var date = request.DateStart.Value; date <= request.DateEnd.Value; date = date.AddMonths(1))
            {
                chart.Labels.Add(date.ToString("MMM yy"));
                
                if(dicStats.TryGetValue((date.Year, date.Month), out TagStatisticsModel stat))
                {
                    dicSeries["total"].Values.Add(stat.Total);
                    dicSeries["average"].Values.Add(average);
                    dicSeries[$"tag:{details.Key}"].Values.Add(stat.TagTotal);

                    foreach(var subTagTotal in stat.SubTagTotals)
                    {
                        dicSeries[$"tag:{subTagTotal.Tag.Key}"].Values.Add(subTagTotal.Amount);
                    }
                }
                else
                {
                    foreach(var mainSerie in chart.Series)
                        dicSeries[mainSerie.Id].Values.Add(0m);
                }    
            }

            var model = new T
            {
                Details = details,
                Transactions = transactions.OrderByDescending(t => t.UserDate).ThenByDescending(t => t.Date),
                Chart = chart,
            };

            return View(viewName, model);
        }
        
        protected async Task<IActionResult> MergeAsync(string tagType, string tag)
        {
            var details = await _api.GetAsync<dto.Model.Tag>($"tags/{tagType.ToCleanQuery()}/{tag.ToCleanQuery()}");
            var listTags = await _api.GetAsync<IEnumerable<dto.Model.Tag>>($"tags/{tagType.ToCleanQuery()}");

            listTags = listTags
                .Where(t => t.Key != tag)
                .OrderBy(t => t.Caption);

            return View("Merge", new MergeTagModel
            {
                SourceTag = details,
                TargetTagList = listTags.Select(t => new SelectListItem { Value = t.Key, Text = t.Caption }),
            });
        }
    }
}