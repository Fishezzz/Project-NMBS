using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Transfer
    {
        public int From_Stop_Id { get; set; }
        public int To_Stop_Id { get; set; }
        public int Transfer_Type { get; set; }
        public int Min_Transfer_Time { get; set; }
        [Obsolete] public string From_Trip_Id { get; set; }
        [Obsolete] public string To_Trip_Id { get; set; }

        public Transfer(string transferString)
        {
            string[] transfer = transferString.Split(',');

            From_Stop_Id = Convert.ToInt32(transfer[0]);
            To_Stop_Id = Convert.ToInt32(transfer[1]);
            Transfer_Type = Convert.ToInt32(transfer[2]);
            Min_Transfer_Time = Convert.ToInt32(transfer[3]);
            From_Trip_Id = transfer[4];
            To_Trip_Id = transfer[5];
        }
    }
}
