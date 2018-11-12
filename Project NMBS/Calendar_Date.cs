using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Calendar_Date
    {
        public int Service_Id { get; set; }
        public int Date { get; set; }
        public int Exception_Type { get; set; }

        public Calendar_Date(string calendar_dateString)
        {
            string[] calendar_date = calendar_dateString.Split(',');

            Service_Id = Convert.ToInt32(calendar_date[0]);
            Date = Convert.ToInt32(calendar_date[1]);
            Exception_Type = Convert.ToInt32(calendar_date[2]);
        }
    }
}
