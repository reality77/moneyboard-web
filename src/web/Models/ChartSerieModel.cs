using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using dto.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;

namespace web.Models
{
    public class ChartModel
    {
        public static readonly string[] DefaultColors = new [] 
        {
            "FFBF00",   // yellow
            "84DE02",   // green
            "FF6700",   // orange
            "966FD6",   // purple
            "7CB9E8",   // light blue   
            "E32636",   // red
            "FFD9B3",   // apricot
        };

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

        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }

        public ChartSerieModel()
        {
            this.Values = new List<decimal>();            
        }        
    }
}