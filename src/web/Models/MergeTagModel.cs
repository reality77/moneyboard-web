using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using dto.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.Models
{
    public class MergeTagModel
    {
        public Tag SourceTag { get; set; }

        [Required]
        public string TargetTagKey { get; set; }

        public IEnumerable<SelectListItem> TargetTagList { get; set; }
    }
}