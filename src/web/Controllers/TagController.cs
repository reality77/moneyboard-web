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
    public class TagController : TagControllerBase
    {
        public TagController(ILogger<TagController> logger, ApiClient api)
        : base(logger, api)
        {
        }

        [Route("{tagType}/{tagKey}")]
        public async Task<IActionResult> Details(string tagType, string tagKey)
        {
            return await base.DetailsInternalAsync<TagModel>(tagType, tagKey);
        }
    }
}
