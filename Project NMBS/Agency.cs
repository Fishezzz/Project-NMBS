using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Agency
    {
        public string Agency_Id { get; set; }
        public string Agency_Name { get; set; }
        public string Agency_Url { get; set; }
        public string Agency_Timezone { get; set; }
        public string Agency_Language { get; set; }
        [Obsolete] public string Agency_Phone { get; set; }

        public Agency(string agencyString)
        {
            string[] agency = agencyString.Split(',');

            Agency_Id = agency[0];
            Agency_Name = agency[1];
            Agency_Url = agency[2];
            Agency_Timezone = agency[3];
            Agency_Language = agency[4];
            Agency_Phone = agency[5];
        }
    }
}
