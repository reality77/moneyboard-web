using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using dto.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace web.Models
{
    public class ChartModel
    {
        public string Type { get; set; }
        public List<ChartSerieModel> Series { get; set; }
        public List<string> Labels { get; set; }

        public ChartModel()
        {
            this.Series = new List<ChartSerieModel>();            
            this.Labels = new List<string>();            
        }
    }

    public class ChartSerieModel
    {
        public string Id { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public List<decimal> Values { get; set; }

        public ChartSerieModel()
        {
            this.Values = new List<decimal>();            
        }        
    }
}