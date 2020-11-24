using System;
using System.Collections.Generic;
using dto.Model;

namespace web.Models.Rules
{
    public class SelectTagTypeModel
    {
        public string OnChangeScript { get; set; }
        public string HtmlFieldId { get; set; }
        public string HtmlFieldName { get; set; }
        public IEnumerable<dto.Model.TagType> AllTagTypes { get; set; }
    }
}