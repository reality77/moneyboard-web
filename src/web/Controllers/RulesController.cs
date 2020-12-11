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

            var conditions = new List<TransactionRecognitionRuleCondition>();
            var actions = new List<TransactionRecognitionRuleAction>();

            ViewBag.AllTagsTypes = await _api.GetAsync<IEnumerable<dto.Model.TagType>>("tagtypes");

            conditions.Add(new TransactionRecognitionRuleCondition
            {
                FieldType = dto.ERecognitionRuleConditionFieldType.Tag,
                FieldName = "payee",
                ValueOperator = dto.ERecognitionRuleConditionOperator.Equals,
                Value = ""
            });

            actions.Add(new TransactionRecognitionRuleAction
            {
                Type = dto.ERecognitionRuleActionType.AddTag,
                Field = "category",
                Value = "",
            });

            rule.Conditions = conditions;
            rule.Actions = actions;

            return View(rule);
        }

        [HttpPost("create")]
        public async Task<IActionResult> PostCreate(dto.Model.TransactionRecognitionRuleEdit rule)
        {
            _logger.LogWarning($"UseOrConditions : {rule.UseOrConditions}");

            foreach(var cond in rule.Conditions)
                _logger.LogWarning($"Condition {cond.FieldType}/{cond.FieldName} {cond.ValueOperator} {cond.Value}");

            foreach(var action in rule.Actions)
                _logger.LogWarning($"Action {action.Type}/{action.Field} => {action.Value}");

            await _api.PostAsync<dto.Model.TransactionRecognitionRule, dto.Model.TransactionRecognitionRuleEdit>($"recognition/rules", rule);

            return RedirectToAction(nameof(List));
        }

        [HttpPost("detecttransactions")]
        public async Task<IActionResult> DetectTransactions(dto.Model.TransactionRecognitionRule rule)
        {
            var transactions = await _api.PostAsync<IEnumerable<ImportedTransaction>, dto.Model.TransactionRecognitionRule>($"recognition/rules/detect", rule);

            return PartialView("ImportedTransactions", transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var rule = await _api.GetAsync<dto.Model.TransactionRecognitionRule>($"recognition/rules/{id}");

            ViewBag.DetectedTransactions = await _api.GetAsync<IEnumerable<ImportedTransaction>>($"recognition/rules/{id}/detect");

            return View(rule);
        }

        [HttpGet("{id}/rescan")]
        public async Task<IActionResult> RescanTransactionsFromRule(int id)
        {
            await _api.PostAsync($"recognition/rules/{id}/scan");

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpGet("addconditionrow")]
        public IActionResult AddConditionRow(int index)
        {
            return PartialView("_PartialRulesRowCondition", index);
        }

        [HttpGet("addactionrow")]
        public IActionResult AddActionRow(int index)
        {
            return PartialView("_PartialRulesRowAction", index);
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
        /// Génère un champ select pour proposer le choix d'un opérateur de comparaison
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
                OnChangeScript = $"on{mode}{field}Changed(this, {index});", // Exemple : onConditionFieldTypeChanged(this, 0);
                ListItems = values.Select(n => new SelectListItem
                {
                    Value = n,
                    Text = n,
                    Selected = (n == value),
                }),
            };

            return PartialView("_PartialSelectRules", model);
        }
        

        /// <summary>
        /// Génère un champ select pour proposer le choix d'un tag
        /// </summary>
        /// <param name="mode">Conditions ou Actions</param>
        /// <param name="index">Index de la condition ou de l'action</param>
        /// <returns></returns>
        [HttpGet("selecttagvalue")]
        public async Task<IActionResult> PartialSelectTagValue(TargetMode mode, string field, int index, string tagType, string value = null)
        {
            var tags = (await _api.GetAsync<IEnumerable<Tag>>($"tags/{tagType}")).OrderBy(t => t.Key);

            var model = new RulesSelectModel
            {
                HtmlFieldId = $"{mode}_{index}_{field}",      // Exemple : Conditions_0_FieldType
                HtmlFieldName = $"{mode}[{index}].{field}",   // Exemple : Conditions[0].FieldType
                OnChangeScript = null,
                ListItems = tags.Select(t => new SelectListItem
                {
                    Value = t.Key,
                    Text = t.Caption ?? t.Key,
                    Selected = (t.Key == value),
                }),
            };

            return PartialView("_PartialSelectRules", model);
        }
                
        /// <summary>
        /// Génère un champ select pour proposer la valeur d'un champ
        /// </summary>
        /// <param name="mode">Conditions ou Actions</param>
        /// <param name="index">Index de la condition ou de l'action</param>
        /// <returns></returns>
        [HttpGet("selectfieldvalue")]
        public IActionResult PartialSelectFieldValue(TargetMode mode, string field, int index, string fieldName, string valueOperator, string value = null)
        {
            Type type = typeof(ImportedTransaction);
            var property = type.GetProperty(fieldName);

            string fieldType;

            switch(property.Name)
            {
                case nameof(ImportedTransaction.Amount):
                    fieldType = "number";
                    break;
                case nameof(ImportedTransaction.ImportNumber):
                    fieldType = "number";
                    break;
                case nameof(ImportedTransaction.Date):
                case nameof(ImportedTransaction.UserDate):
                {
                    if(!string.IsNullOrWhiteSpace(valueOperator))
                    {
                        switch(Enum.Parse<dto.ERecognitionRuleConditionOperator>(valueOperator))
                        {
                            case dto.ERecognitionRuleConditionOperator.DayEquals:
                            case dto.ERecognitionRuleConditionOperator.WeekEquals:
                            case dto.ERecognitionRuleConditionOperator.MonthEquals:
                            case dto.ERecognitionRuleConditionOperator.YearEquals:
                            case dto.ERecognitionRuleConditionOperator.DayOfWeekEquals:
                            case dto.ERecognitionRuleConditionOperator.DayNear:
                                fieldType = "number";
                                break;
                            default:
                                fieldType = "date";
                                break;
                        }
                    }
                    else
                        fieldType = "date";
                }
                    break;
                default:
                    fieldType = "text";
                    break;
            }

            var model = new RulesInputModel
            {
                HtmlFieldId = $"{mode}_{index}_{field}",      // Exemple : Conditions_0_FieldType
                HtmlFieldName = $"{mode}[{index}].{field}",   // Exemple : Conditions[0].FieldType
                OnChangeScript = null,
                HtmlType = fieldType,
                Value = value,
            };

            return PartialView("_PartialInputRules", model);
        }
    }
}
