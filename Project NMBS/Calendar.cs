using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Calendar
    {
        public int Service_Id { get; set; }
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }
        public int Start_Date { get; set; }
        public int End_Date { get; set; }

        public Calendar(string calendarString)
        {
            string[] calendar = calendarString.Split(',');

            Service_Id = Convert.ToInt32(calendar[0]);
            Monday = Convert.ToInt32(calendar[1]);
            Tuesday = Convert.ToInt32(calendar[2]);
            Wednesday = Convert.ToInt32(calendar[3]);
            Thursday = Convert.ToInt32(calendar[4]);
            Friday = Convert.ToInt32(calendar[5]);
            Saturday = Convert.ToInt32(calendar[6]);
            Sunday = Convert.ToInt32(calendar[7]);
            Start_Date = Convert.ToInt32(calendar[8]);
            End_Date = Convert.ToInt32(calendar[9]);
        }
    }
}
