using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASAInformationBot.Model
{
    public class MarsRoverPhotos
    {
        public List<Photos> photos { get; set; }
    }
    public class Photos
    {
        public Camera camera { get; set; }
        public string img_src { get; set; }
        public string earth_date { get; set; }
        public Rover rover { get; set; }
    }
    public class Camera
    {
        public string name { get; set; }
        public string full_name { get; set; }
    }
    public class Rover
    {
        public string name { get; set; }
        public string landing_date { get; set; }
        public string launch_date { get; set; }
        public string status { get; set; }

    }
}
