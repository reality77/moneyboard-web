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
    [Route("rules")]
    [Authorize]
    public class RulesController : Controller
    {
        private readonly ILogger<RulesController> _logger;
        
        private readonly ApiClient _api;

        public RulesController(ILogger<RulesController> logger, ApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        [Route("")]
        public async Task<IActionResult> List(string filterTagTypeKey = null, string filterTagKey = null)
        {
            var rules = await _api.GetAsync<IEnumerable<dto.Model.TransactionRecognitionRule>>("recognition/rules");
            
            if(!string.IsNullOrWhiteSpace(filterTagTypeKey) && !string.IsNullOrWhiteSpace(filterTagKey))
            {
                rules = rules.Where(r => r.Conditions.Any(c => c.FieldType == dto.ERecognitionRuleConditionFieldType.Tag 
                    && c.FieldName == filterTagTypeKey 
                    && c.Value == filterTagKey 
                    && c.ValueOperator == dto.ERecognitionRuleConditionOperator.Equals));
            }
            
            return View(rules);
        }

        
        [HttpGet("create")]
        public async Task<IActionResult> Create(string filterTagTypeKey = null, string filterTagKey = null)
        {
            var rule = new dto.Model.TransactionRecognitionRule();

            var list = new List<TransactionRecognitionRuleCondition>();

            ViewBag.AllTagsTypes = await _api.GetAsync<IEnumerable<dto.Model.TagType>>("tagtypes");

            list.Add(new TransactionRecognitionRuleCondition
            {
                FieldType = dto.ERecognitionRuleConditionFieldType.Tag,
                FieldName = "payee",
                ValueOperator = dto.ERecognitionRuleConditionOperator.Equals,
                Value = ""
            });

            list.Add(new TransactionRecognitionRuleCondition
            {
                FieldType = dto.ERecognitionRuleConditionFieldType.Tag,
                FieldName = "category",
                ValueOperator = dto.ERecognitionRuleConditionOperator.Equals,
                Value = "test"
            });


            rule.Conditions = list;

            return View(rule);
        }

        [HttpPost("create")]
        public async Task<IActionResult> PostCreate(dto.Model.TransactionRecognitionRule rule)
        {
            var list = new List<TransactionRecognitionRuleCondition>();

            _logger.LogInformation($"UseOrConditions : {rule.UseOrConditions}");

            foreach(var cond in rule.Conditions)
                _logger.LogInformation($"Condition {cond.FieldType}/{cond.FieldName} {cond.ValueOperator} {cond.Value}");

            foreach(var action in rule.Actions)
                _logger.LogInformation($"Action {action.Type}/{action.Field} => {action.Value}");

            return RedirectToAction("List");
        }
    }
}
