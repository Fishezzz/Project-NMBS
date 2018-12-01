using GTFS.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
{
    /// <summary>
    /// The Trip.Id string split into it's components, which are converted to their corresponding objects. This makes it easier to use the Trip.Id property.
    /// </summary>
    public class TripId
    {
        ////// TODO: DateTime veranderen door GTFS.Entities.TimeOfDay


        /// <summary>
        /// Administration code of this trip. (The Belgium NMBS code is "88____")
        /// </summary>
		public string AdministrationCode { get; set; }
        /// <summary>
        /// The category code of this trip.
        /// </summary>
		public string CategoryCode { get; set; }
        /// <summary>
        /// The line number for the route of this train.
        /// </summary>
		public string LineNumber { get; set; }
        /// <summary>
        /// The station where the train starts it's route.
        /// </summary>
		public Stop FirstStop { get; set; }
        /// <summary>
        /// The station where the train ends it's route.
        /// </summary>
		public Stop LastStop { get; set; }
        /// <summary>
        /// The trip time from the first stop till the last stop.
        /// </summary>
		public TimeSpan TripLength { get; set; }
        /// <summary>
        /// Time when the train arrives at the last stop of it's route.
        /// </summary>
		public DateTime ArrivalTimeLastStop { get; set; }
        /// <summary>
        /// Last date when the route is serviced in the given GTFS data set.
        /// </summary>
		public DateTime LastServiceDay { get; set; }
        /// <summary>
        /// The full ServiceId as a string.
        /// </summary>
        public string TripIdString { get; set; }

        /// <summary>
        /// Constructor for TripId. Creates a new TripId object
        /// </summary>
        /// <param name="tripId">The plain text string 'Trip.Id'</param>
        public TripId(string tripId)
        {
            string[] s = tripId.Split(':');
            AdministrationCode = s[0];
            CategoryCode = s[1];
            LineNumber = s[2];
            FirstStop = (from station in MainWindow._stops where station.Id.TrimStart('S').Split('_')[0] == s[3] select station).FirstOrDefault();
            LastStop = (from station in MainWindow._stops where station.Id.TrimStart('S').Split('_')[0] == s[4] select station).FirstOrDefault();
            TripLength = TimeSpan.FromMinutes(Convert.ToInt32(s[5]));
            ArrivalTimeLastStop = new DateTime().AddHours(Convert.ToInt32(s[6]) / 100).AddMinutes(Convert.ToInt32(s[6]) % 100);
            LastServiceDay = DateTime.ParseExact(s[7], "yyyyMMdd", new CultureInfo("fr-FR"));
            TripIdString = tripId;
        }

        /// <summary>
        /// Returns the full Trip.Id as a string.
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return TripIdString;
        }
    }



    /// <summary>
    /// Inherited from Trip, but the type of the Id property has changed from string to TripId.
    /// </summary>
    public class TripExtra : Trip
    {
        /// <summary>
        /// Replaces the string property Id from Trip.
        /// </summary>
        public new TripId Id { get; set; }

        /// <summary>
        /// Constructor for TripExtra. Creates a new TripExtra object.
        /// </summary>
        /// <param name="trip">The 'old' Trip that gets replaced.</param>
        public TripExtra(Trip trip)
        {
            Id = new TripId(trip.Id);
            RouteId = trip.RouteId;
            ServiceId = trip.ServiceId;
            Headsign = trip.Headsign;
            ShortName = trip.ShortName;
            Direction = trip.Direction;
            BlockId = trip.BlockId;
            ShapeId = trip.ShapeId;
            AccessibilityType = trip.AccessibilityType;
        }
    }



    public static class TripExtensionMethods
    {
        /// <summary>
        /// Converts a Trip into a TripExtra
        /// </summary>
        /// <param name="trip">The Trip that will be converted to a TripExtra</param>
        /// <returns>A TripExtra</returns>
        public static TripExtra ToTripExtra(this Trip trip)
        {
            try
            {
                if (trip is Trip)
                    return new TripExtra(trip);
                else
                    throw new NotATripException();
            }
            catch (NotATripException NATex)
            {
                Debug.WriteLine($"'trip' is not of the desired type 'Trip' ({NATex.Message})");
                return null;
            }
        }

        /// <summary>
        /// Converts a List&lt;Trip&gt; into a List&lt;TripExtra&gt;
        /// </summary>
        /// <param name="trips">The List&lt;Trip&gt; that will be converted into a List&lt;TripExtra&gt;</param>
        /// <returns>A List&lt;TripExtra&gt;</returns>
        public static List<TripExtra> ToTripExtraList(this List<Trip> trips)
        {
            List<TripExtra> tripsExtra = new List<TripExtra>();
            foreach (Trip trip in trips)
                tripsExtra.Add(trip.ToTripExtra());
            return tripsExtra;
        }
    }
}