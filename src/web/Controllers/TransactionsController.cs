using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using dto.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            var allTagTypes = await _api.GetAsync<IEnumerable<dto.Model.TagType>>($"tagtypes");
            
            this.ViewBag.AllTagTypes = allTagTypes.OrderBy(tt => tt.Key).Select(tt => new SelectListItem
                {
                    Value = tt.Key,
                    Text = $"{tt.Key} - {tt.Caption}"
                });

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

        [HttpPost]
        [Route("{id}/tag/remove")]
        public async Task<IActionResult> RemoveTag(int id, [FromBody] Tag tag)
        {
            await _api.DeleteAsync($"transactions/{id}/tag/{tag.TypeKey}/{tag.Key}");

            return NoContent();
        }      

        [HttpPost]
        [Route("{id}/tag/add")]
        public async Task<IActionResult> AddTag(int id, [Bind("TypeKey,Key")] Tag tag)
        {
            var realTag = await _api.GetAsync<Tag>($"tags/{tag.TypeKey}/{tag.Key}");

            await _api.PutAsync<TransactionTag>($"transactions/{id}/tag", new TransactionTag
            {
                TagTypeKey = realTag.TypeKey,
                TagKey = realTag.Key,
                IsManual = true,
            });

            return NoContent();
        }   
    }
}
