using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Trip
    {
        public int Route_Id { get; set; }
        public int Service_Id { get; set; }
        public string Trip_Id { get; set; }
        public string Trip_Headsign { get; set; }
        public int Trip_Short_Name { get; set; }
        [Obsolete] public string Direction_Id { get; set; }
        public int Block_Id { get; set; }
        [Obsolete] public string Shape_Id { get; set; }
        public int Trip_Type { get; set; }

        public Trip(string tripString)
        {
            string[] trip = tripString.Split(',');

            Route_Id = Convert.ToInt32(trip[0]);
            Service_Id = Convert.ToInt32(trip[1]);
            Trip_Id = trip[2];
            Trip_Headsign = trip[3];
            Trip_Short_Name = Convert.ToInt32(trip[4]);
            Direction_Id = trip[5];
            Block_Id = Convert.ToInt32(trip[6]);
            Shape_Id = trip[7];
            Trip_Type = Convert.ToInt32(trip[8]);
        }
    }
}
