using GTFS.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_NMBS
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
            string tripId = MainWindow._trips.Values.Where(x => x.RouteId == route.Id).ToList().Select(x => x.Id).ToArray().First();
            List<string> stopIds = MainWindow._feedStatic.StopTimes.GetForTrip(tripId).Select(x => x.StopId).ToList();
            foreach (string stopId in stopIds)
            {
                Stop stopForDictionary;
                MainWindow._stops.TryGetValue(stopId, out stopForDictionary);
                StopList.Add(stopId, stopForDictionary);
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
}
