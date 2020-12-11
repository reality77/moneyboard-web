using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.Models.Rules
{
    public class RulesInputModel
    {
        public string OnChangeScript { get; set; }
        public string HtmlFieldId { get; set; }
        public string HtmlFieldName { get; set; }
        public string HtmlType { get; set; }
        public string Value { get; set; }
    }
}