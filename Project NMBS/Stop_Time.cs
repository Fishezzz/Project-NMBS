using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Stop_Time
    {
        public string Trip_Id { get; set; }
        public string Arrival_Time { get; set; }
        public string Departure_Time { get; set; }
        public int Stop_Id { get; set; }
        public int Stop_Sequence { get; set; }
        [Obsolete] public string Stop_Headsign { get; set; }
        public int Pickup_Type { get; set; }
        public int Drop_Off_Type { get; set; }
        [Obsolete] public string Shape_Dist_Traveled { get; set; }

        public Stop_Time(string stop_timeString)
        {
            string[] stop_time = stop_timeString.Split(',');

            Trip_Id = stop_time[0];
            Arrival_Time = stop_time[1];
            Departure_Time = stop_time[2];
            Stop_Id = Convert.ToInt32(stop_time[3]);
            Stop_Sequence = Convert.ToInt32(stop_time[4]);
            Stop_Headsign = stop_time[5];
            Pickup_Type = Convert.ToInt32(stop_time[6]);
            Drop_Off_Type = Convert.ToInt32(stop_time[7]);
            Shape_Dist_Traveled = stop_time[8];
        }
    }
}
