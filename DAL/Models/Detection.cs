using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Detection:BaseClass
    {
        public DateTime Timestamp { get; set; }
        public string CameraId { get; set; }
        public string ObjectType { get; set; }
        public float Confidence { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public string ImagePath { get; set; }
        public virtual Camera Camera { get; set; }
    }
}
