using GTFS.Entities;
using GTFS.Entities.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishezzz
{
    /// <summary>
    /// Inherited from Route, but a List&lt;TripExtra&gt; TripList has been added.
    /// </summary>
    public class RouteExtra : Route
    {
        /// <summary>
        /// A Dictionary&lt;string, Stop&gt; containing all the Stops in this Route.
        /// </summary>
        public Dictionary<string, Stop> StopList { get; }

        /// <summary>
        /// Constructor for RouteExtra. Creates a RouteExtra object.
        /// </summary>
        /// <param name="route">The 'old' Route that gets replaced.</param>
        public RouteExtra(Route route)
        {
            Id = route.Id;
            AgencyId = route.AgencyId;
            ShortName = route.ShortName;
            LongName = route.LongName;
            Description = route.Description;
            Type = route.Type;
            Url = route.Url;
            Color = route.Color;
            TextColor = route.TextColor;

            StopList = new Dictionary<string, Stop>();
            string tripId = Project_NMBS.MainWindow._trips.Values.Where(x => x.RouteId == route.Id).ToList().Select(x => x.Id).ToArray().First();
            List<StopTime> stopTimes = Project_NMBS.MainWindow._feedStatic.StopTimes.GetForTrip(tripId).Where(x => x.DropOffType == DropOffType.Regular || x.PickupType == PickupType.Regular).ToList();
            foreach (StopTime stopTime in stopTimes)
            {
                Project_NMBS.MainWindow._stops.TryGetValue(stopTime.StopId, out Stop stop);
                StopList.Add(stopTime.StopId, stop);
            }
        }
    }

    public static class RouteExtensionMethods
    {
        /// <summary>
        /// Converts a Route into a RouteExtra
        /// </summary>
        /// <param name="route">The Route that will be converted to a RouteExtra</param>
        /// <returns>A RouteExtra</returns>
        public static RouteExtra ToRouteExtra(this Route route)
        {
            try
            {
                if (route is Route)
                    return new RouteExtra(route);
                else
                    throw new NotARouteException();
            }
            catch (NotARouteException NARex)
            {
                Debug.WriteLine($"'route' is not of the desired type 'Route' ({NARex.Message})");
                return null;
            }
        }

        /// <summary>
        /// Converts a List&lt;Route&gt; into a Dictionary&lt;string, RouteExtra&gt;
        /// </summary>
        /// <param name="routes">The List&lt;Route&gt; that will be converted into a Dictionary&lt;string, RouteExtra&gt;</param>
        /// <returns>A Dictionary&lt;string, RouteExtra&gt;</returns>
        public static Dictionary<string, RouteExtra> ToRouteExtraDictionary(this List<Route> routes)
        {
            Dictionary<string, RouteExtra> routesExtra = new Dictionary<string, RouteExtra>();
            foreach (Route route in routes)
                routesExtra.Add(route.Id, route.ToRouteExtra());
            return routesExtra;
        }
        /// <summary>
        /// Converts a Dictionary&lt;string, Route&gt; into a Dictionary&lt;string, RouteExtra&gt;
        /// </summary>
        /// <param name="routes">The Dictionary&lt;string, Route&gt; that will be converted into a Dictionary&lt;string, RouteExtra&gt;</param>
        /// <returns>A Dictionary&lt;string, RouteExtra&gt;</returns>
        public static Dictionary<string, RouteExtra> ToRouteExtraDictionary(this Dictionary<string, Route> routes)
        {
            Dictionary<string, RouteExtra> routesExtra = new Dictionary<string, RouteExtra>();
            foreach (KeyValuePair<string, Route> kVP in routes)
                routesExtra.Add(kVP.Key, kVP.Value.ToRouteExtra());
            return routesExtra;
        }
    }

    public static class Extensions
    {
        public static string Trimmed(this string stopId)
        {
            try
            {
                return stopId.TrimStart('S').Split('_')[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return stopId;
            }
        }
    }
}
