using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate.Models
{
    public class Weather
    {
        [JsonProperty("hourly")]
        public Hourly Hourly { get; set; }
    }

    public class Hourly
    {
        [JsonProperty("temperature_2m")]
        public List<double> Temperature_2m { get; set; }
    }


}
