using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Stop_Time_Override
    {
        public string Trip_Id { get; set; }
        public int Stop_Sequence { get; set; }
        public int Service_Id { get; set; }
        public string Stop_Id { get; set; }

        public Stop_Time_Override(string stop_time_overrideString)
        {
            string[] stop_time_override = stop_time_overrideString.Split(',');

            Trip_Id = stop_time_override[0];
            Stop_Sequence = Convert.ToInt32(stop_time_override[1]);
            Service_Id = Convert.ToInt32(stop_time_override[2]);
            Stop_Id = stop_time_override[3];
        }
    }
}
