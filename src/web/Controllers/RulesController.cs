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

        
        [Route("create")]
        public async Task<IActionResult> Create(string filterTagTypeKey = null, string filterTagKey = null)
        {
            var rule = new dto.Model.TransactionRecognitionRule();

            var list = new List<TransactionRecognitionRuleCondition>();

            list.Add(new TransactionRecognitionRuleCondition
            {
                FieldType = dto.ERecognitionRuleConditionFieldType.Tag,
                FieldName = "test",
                ValueOperator = dto.ERecognitionRuleConditionOperator.Greater,
                Value = "150"
            });

            rule.Conditions = list;

            return View(rule);
        }
    }
}
