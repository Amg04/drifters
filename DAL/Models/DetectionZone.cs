using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class DetectionZone
    {
        public string Name { get; set; } = string.Empty;
        public List<Point> Polygon { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
    }
}
