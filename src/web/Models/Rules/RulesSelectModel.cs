using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.Models.Rules
{
    public class RulesSelectModel
    {
        public string OnChangeScript { get; set; }
        public string HtmlFieldId { get; set; }
        public string HtmlFieldName { get; set; }
        public IEnumerable<SelectListItem> ListItems { get; set; }
    }
}