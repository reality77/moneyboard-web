using System;
using System.Collections.Generic;
using dto.Model;

namespace web.Models
{
    public class TagModel
    {
        public TagDetails Details { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
        public IEnumerable<TagStatisticsModel> Statistics { get; set; }
    }
}