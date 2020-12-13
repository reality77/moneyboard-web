using System;
using System.Collections.Generic;
using dto.Model;

namespace web.Models
{
    public class TagListModel
    {
        public string TagType { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public ChartModel Chart { get; set; }
    }
}