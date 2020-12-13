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
            "7CB9E8",   // light blue
            "FFBF00",   // yellow
            "00994D",   // green
            "966FD6",   // purple
            "FF6700",   // orange
            "3355FF",   // ultramarine blue
            "FFD9B3",   // apricot
            "E32636",   // red
            "84DE02",   // lawn green
            "E566FF",   // rose
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