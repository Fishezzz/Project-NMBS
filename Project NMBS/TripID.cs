using GTFS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    public class TripID
    {
        public TripID(string tripId, string tripId1, string tripId2, string tripId3, Stop beginStation, Stop endStation, string tripId6, DateTime time)
        {
            string[] tripIdArray = tripId.Split(':');

            TripIdFull = tripId;
            TripId1 = tripIdArray[0];
            TripId2 = tripIdArray[1];
            TripId3 = tripIdArray[2];

            TripId6 = tripIdArray[5];

            DepartureDate = new DateTime();
        }
        // 88____:A71::8821402:8400526:3:650:20181208
        public string TripIdFull { get; }
        public string TripId1 { get; }
        public string TripId2 { get; }
        public string TripId3 { get; }
        public Stop BeginStation { get; }
        public Stop EndStation { get; }
        public string TripId6 { get; }
        public DateTime DepartureDate { get; }
    }
}
