using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTFS.Entities;
using GTFS.Entities.Enumerations;

namespace Project_NMBS
{
    public class TripExpanded : Trip
    {
        public TripExpanded(Trip pTrip)
        {
            Id = pTrip.Id;
            RouteId = pTrip.RouteId;
            ServiceId = pTrip.ServiceId;
            Headsign = pTrip.Headsign;
            ShortName = pTrip.ShortName;
            Direction = pTrip.Direction;
            BlockId = pTrip.BlockId;
            ShapeId = pTrip.ShapeId;
        }
    }
}
