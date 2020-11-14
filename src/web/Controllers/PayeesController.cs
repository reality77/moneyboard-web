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
    public class PayeesController : TagControllerBase
    {
        public PayeesController(ILogger<PayeesController> logger, ApiClient api)
        : base(logger, api)
        {
        }
        
        [Route("{key}")]
        public async Task<IActionResult> Details(string key)
        {
            return await base.DetailsInternalAsync<PayeeModel>("payee", key);
        }
    }
}
