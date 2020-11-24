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
using web.Models.Rules;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.Controllers
{
    [Route("rules")]
    [Authorize]
    public class RulesController : Controller
    {
        public enum TargetMode
        {
            Conditions,
            Actions,
        }

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

            rule.Conditions = list;

            return View(rule);
        }

        [HttpPost("create")]
        public async Task<IActionResult> PostCreate(dto.Model.TransactionRecognitionRule rule)
        {
            _logger.LogWarning($"UseOrConditions : {rule.UseOrConditions}");

            foreach(var cond in rule.Conditions)
                _logger.LogWarning($"Condition {cond.FieldType}/{cond.FieldName} {cond.ValueOperator} {cond.Value}");

            foreach(var action in rule.Actions)
                _logger.LogWarning($"Action {action.Type}/{action.Field} => {action.Value}");

            return View(rule); // pour debug
        }

        /// <summary>
        /// Génère un champ select pour proposer le choix d'un type de tag
        /// </summary>
        /// <param name="mode">Conditions ou Actions</param>
        /// <param name="index">Index de la condition ou de l'action</param>
        /// <returns></returns>
        [HttpGet("selecttagtypes")]
        public async Task<IActionResult> PartialSelectTagTypes(TargetMode mode, string field, int index, string value = null)
        {
            var model = new RulesSelectModel
            {
                HtmlFieldId = $"{mode}_{index}_{field}",      // Exemple : Conditions_0_FieldType
                HtmlFieldName = $"{mode}[{index}].{field}",   // Exemple : Conditions[0].FieldType
                OnChangeScript = $"on{mode}{field}Changed(this, {index});", // Exemple : onConditionFieldTypeChanged(this, 0);
                ListItems = (await _api.GetAsync<IEnumerable<dto.Model.TagType>>("tagtypes")).OrderBy(tt => tt.Key).Select(tt => new SelectListItem
                {
                    Value = tt.Key,
                    Text = tt.Caption ?? tt.Key,
                    Selected = (tt.Key == value),
                }),
            };

            return PartialView("_PartialSelectRules", model);
        }


        /// <summary>
        /// Génère un champ select pour proposer le choix d'un champ
        /// </summary>
        /// <param name="mode">Conditions ou Actions</param>
        /// <param name="index">Index de la condition ou de l'action</param>
        /// <returns></returns>
        [HttpGet("selectfields")]
        public IActionResult PartialSelectFields(TargetMode mode, string field, int index, string value = null)
        {
            Type type = typeof(ImportedTransaction);
            var properties = type.GetProperties().AsEnumerable().OrderBy(p => p.Name);

            var model = new RulesSelectModel
            {
                HtmlFieldId = $"{mode}_{index}_{field}",      // Exemple : Conditions_0_FieldType
                HtmlFieldName = $"{mode}[{index}].{field}",   // Exemple : Conditions[0].FieldType
                OnChangeScript = $"on{mode}{field}Changed(this, {index});", // Exemple : onConditionFieldTypeChanged(this, 0);
                ListItems = properties.Select(p => new SelectListItem
                {
                    Value = p.Name,
                    Text = p.Name,
                    Selected = (p.Name == value),
                }),
            };

            return PartialView("_PartialSelectRules", model);
        }

        /// <summary>
        /// Génère un champ select pour proposer le choix d'un opérateur
        /// </summary>
        /// <param name="mode">Conditions ou Actions</param>
        /// <param name="index">Index de la condition ou de l'action</param>
        /// <returns></returns>
        [HttpGet("selectvalueoperator")]
        public IActionResult PartialSelectValueOperator(TargetMode mode, string field, int index, string value = null)
        {
            var values = Enum.GetNames(typeof(dto.ERecognitionRuleConditionOperator)).AsEnumerable().OrderBy(n => n);

            var model = new RulesSelectModel
            {
                HtmlFieldId = $"{mode}_{index}_{field}",      // Exemple : Conditions_0_FieldType
                HtmlFieldName = $"{mode}[{index}].{field}",   // Exemple : Conditions[0].FieldType
                OnChangeScript = null, // $"on{mode}{field}Changed(this, {index});", // Exemple : onConditionFieldTypeChanged(this, 0);
                ListItems = values.Select(n => new SelectListItem
                {
                    Value = n,
                    Text = n,
                    Selected = (n == value),
                }),
            };

            return PartialView("_PartialSelectRules", model);
        }        
    }
}
