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
    [Route("tags")]
    [Authorize]
    public class TagsController : TagControllerBase
    {
        public TagsController(ILogger<TagsController> logger, ApiClient api)
        : base(logger, api)
        {
        }

        [Route("{tagType}")]
        public async Task<IActionResult> List(string tagType)
        {
            return await base.ListInternalAsync(tagType);
        }

        [Route("{tagType}/select")]
        public async Task<IActionResult> PartialListForSelect(string tagType)
        {
            var model = await _api.GetAsync<IEnumerable<dto.Model.Tag>>($"tags/{tagType.ToCleanQuery()}");
            return PartialView("_TagKeySelection", model);
        }

        [Route("{tagType}/{tagKey}")]
        public async Task<IActionResult> Details(string tagType, string tagKey)
        {
            if(tagType == "payee")
                return RedirectToAction("Details", "Payees", new { key = tagKey });

            return await base.DetailsInternalAsync<TagModel>(tagType, tagKey);
        }

        [Route("{tagType}/{tagKey}/merge")]
        public async Task<IActionResult> Merge(string tagType, string tagKey)
        {
            return await base.MergeAsync(tagType, tagKey);
        }

        [HttpPost]
        [Route("{tagType}/{tagKey}/merge")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostMerge(string tagType, string tagKey, [Bind("TargetTagKey")] MergeTagModel model)
        {
            if(!ModelState.IsValid)
                return await base.MergeAsync(tagType, tagKey);

            await _api.PostAsync($"tags/{tagType.ToCleanQuery()}/{tagKey.ToCleanQuery()}/merge?target={model.TargetTagKey.ToCleanQuery()}", null, "application/json");

            return RedirectToAction("Details", new { tagType = tagType, tagKey = model.TargetTagKey });
        }
    }
}
