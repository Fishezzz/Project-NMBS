using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Route
    {
        public int Route_Id { get; set; }
        public string Agency_Id { get; set; }
        public string Route_Short_Name { get; set; }
        public string Route_Long_Name { get; set; }
        public string Route_Desc { get; set; }
        public int Route_Type { get; set; }
        public string Route_Url { get; set; }
        public string Route_Color { get; set; }
        public string Route_Text_Color { get; set; }

        public Route(string routeString)
        {
            string[] route = routeString.Split(',');

            Route_Id = Convert.ToInt32(route[0]);
            Agency_Id = route[1];
            Route_Short_Name = route[2];
            Route_Long_Name = route[3];
            Route_Desc = route[4];
            Route_Type = Convert.ToInt32(route[5]);
            Route_Url = route[6];
            Route_Color = route[7];
            Route_Text_Color = route[8];
        }
    }
}
