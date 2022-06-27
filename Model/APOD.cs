using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInformationBot.Model
{
    public  class APOD
    {
        public string date { get; set; }
        public string explanation { get; set; }
        //public string hdurl { get; set; }
        public string media_type { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }
}
