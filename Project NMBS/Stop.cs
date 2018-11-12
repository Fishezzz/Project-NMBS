using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Stop
    {
        public string Stop_Id { get; set; }
        public string Stop_Code { get; set; }
        public string Stop_Name { get; set; }
        public string Stop_Desc { get; set; }
        public double Stop_Lat { get; set; }
        public double Stop_Long { get; set; }
        public string Zone_Id { get; set; }
        public string Stop_Url { get; set; }
        public int Location_Type { get; set; }
        public string Parent_Station { get; set; }
        public string Platform_Code { get; set; }

        public Stop(string stopString)
        {
            string[] stop = stopString.Split(',');
            stop[4] = string.Join(",", stop[4].Split('.'));
            stop[5] = string.Join(",", stop[5].Split('.'));

            Stop_Id = stop[0];
            Stop_Code = stop[1];
            Stop_Name = stop[2];
            Stop_Desc = stop[3];
            Stop_Lat = Convert.ToDouble(stop[4]);
            Stop_Long = Convert.ToDouble(stop[5]);
            Zone_Id = stop[6];
            Stop_Url = stop[7];
            Location_Type = Convert.ToInt32(stop[8]);
            Parent_Station = stop[9];
            Platform_Code = stop[10];
        }
    }
}
