using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishezzz
{
    class StopTimeOverride
    {
        public string Trip_Id { get; set; }
        public uint Stop_Sequence { get; set; }
        public string Service_Id { get; set; }
        public string Stop_Id { get; set; }

        public StopTimeOverride(string stop_time_overrideString)
        {
            string[] stop_time_override = stop_time_overrideString.Split(',');

            Trip_Id = stop_time_override[0];
            Stop_Sequence = Convert.ToUInt32(stop_time_override[1]);
            Service_Id = stop_time_override[2];
            Stop_Id = stop_time_override[3];
        }
    }
}
