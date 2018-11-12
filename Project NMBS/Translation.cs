using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    class Translation
    {
        public string Trans_Id { get; set; }
        public string Language { get; set; }
        public string Trans { get; set; }

        public Translation(string translationString)
        {
            string[] translation = translationString.Split(',');

            Trans_Id = translation[0];
            Language = translation[1];
            Trans = translation[2];
        }
    }
}
