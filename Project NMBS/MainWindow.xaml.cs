using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GTFS;
using GTFS.Attributes;
using GTFS.DB;
using GTFS.DB.Memory;
using GTFS.Entities;
using GTFS.Entities.Collections;
using GTFS.Entities.Enumerations;
using GTFS.Exceptions;
using GTFS.Fields;
using GTFS.Filters;
using GTFS.IO;
using GTFS.IO.CSV;
using GTFS.StopsToShape;
using GTFS.Validation;

using System.Net;
using ProtoBuf;
using transit_realtime;

using Fishezzz;
using System.Threading;

namespace Project_NMBS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GTFSReader<GTFSFeed> _reader;
        /// <summary>
        /// _feed.StopTimes.StopId is TRIMMED
        /// </summary>
        public static GTFSFeed _feedStatic;
        FeedMessage _feedRealtime;

        /// <summary>
        /// Key: Stop_Id (string) NOT TRIMMED!
        /// <para/>
        /// Value: Stop (Stop)
        /// </summary>
        public static Dictionary<string, Stop> _stops;
        /// <summary>
        /// Key: Trip_Id (string)
        /// <para/>
        /// Value: Trip (Trip)
        /// </summary>
        public static Dictionary<string, Trip> _trips;
        /// <summary>
        /// Key: Route_Id (string)
        /// <para/>
        /// Value: Route (Route)
        /// </summary>
        Dictionary<string, RouteExtra> _routes;
        /// <summary>
        /// Key: Agency_Id (string)
        /// <para/>
        /// Value: Agency (Agency)
        /// </summary>
        Dictionary<string, Agency> _agencies;
        /// <summary>
        /// Key: Service_Id (string)
        /// <para/>
        /// Value: Calendar (Calendar)
        /// </summary>
        Dictionary<string, GTFS.Entities.Calendar> _calendars;
        /// <summary>
        /// Key: From_Stop_Id (string) TRIMMED
        /// <para/>
        /// Value: Transfer (Transfer)
        /// </summary>
        Dictionary<string, Transfer> _transfers;

        /// <summary>
        /// Used when 'Service_Id' is known, but a specific 'Date' needs to be found.
        /// <para/>
        /// Key: Service_Id (string)
        /// <para/>
        /// Value-Key: Date (DateTime)
        /// <para/>
        /// Value-Value: Exception_Type (ExceptionType)
        /// </summary>
        Dictionary<string, Dictionary<DateTime, ExceptionType>> _calendarDates;
        /// <summary>
        /// Key: Trans_Id (string)
        /// <para/>
        /// Value-Key: Language (string)
        /// <para/>
        /// Value-Value: Trans (string)
        /// </summary>
        Dictionary<string, Dictionary<string, string>> _translations;
        /// <summary>
        /// StopId is WITH an __
        /// </summary>
        List<StopTimeOverride> _stopTimeOverrides;

        Stop _searchBeginStationRouteplanner;
        Stop _searchEndStationRoutePlanner;
        Stop _searchStationTripviewer;
        Stop _searchStationRealtime;
        Stop _searchStationRoutefinder;

        //  fr      nl      de      en
        const string _LANG = "nl";

        StringBuilder _sb;
        const char WS = ' ';
        const string NA = "N/A";
        string[] RLNS = { "--" };

        enum SelectedLv { BeginStationRouteplanner = 1, EndStationRouteplanner = 2, StationTripviewer = 3, StationRealtime = 4, StationRoutefinder = 5 }

        public MainWindow()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");

            // Create StringBuilder
            _sb = new StringBuilder();

            // Read GTFS Static files
            _reader = new GTFSReader<GTFSFeed>(strict: false);
            _feedStatic = _reader.Read(new DirectoryInfo("GTFS"));

            // Read extra files and create lists with objects
            string[] translationsRaw = File.ReadAllLines("GTFS\\translations.txt");
            string[] stop_time_overridesRaw = File.ReadAllLines("GTFS\\stop_time_overrides.txt");

            // Create lists with objects
            _stops = _feedStatic.Stops.ToDictionary(x => x.Id, x => x);
            _trips = _feedStatic.Trips.ToDictionary(x => x.Id, x => x);
            _routes = _feedStatic.Routes.ToDictionary(x => x.Id, x => x).ToRouteExtraDictionary();
            _agencies = _feedStatic.Agencies.ToDictionary(x => x.Id, x => x);
            _calendars = _feedStatic.Calendars.ToDictionary(x => x.ServiceId, x => x);
            _transfers = _feedStatic.Transfers.ToDictionary(x => x.FromStopId, x => x);

            _calendarDates = new Dictionary<string, Dictionary<DateTime, ExceptionType>>();
            foreach (string serviceId in _calendars.Keys)
                _calendarDates.Add(serviceId, new Dictionary<DateTime, ExceptionType>());
            foreach (CalendarDate cd in _feedStatic.CalendarDates)
                _calendarDates[cd.ServiceId].Add(cd.Date, cd.ExceptionType);
            
            _translations = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 1; i < translationsRaw.Length;)
            {
                string key = new Translation(translationsRaw[i]).Trans_Id;
                Dictionary<string, string> value = new Dictionary<string, string>();

                Translation valueValue = new Translation(translationsRaw[i++]);
                value.Add(valueValue.Language, valueValue.Trans);
                valueValue = new Translation(translationsRaw[i++]);
                value.Add(valueValue.Language, valueValue.Trans);
                valueValue = new Translation(translationsRaw[i++]);
                value.Add(valueValue.Language, valueValue.Trans);
                valueValue = new Translation(translationsRaw[i++]);
                value.Add(valueValue.Language, valueValue.Trans);

                _translations.Add(key, value);
            }

            _stopTimeOverrides = new List<StopTimeOverride>();
            for (int i = 1; i < stop_time_overridesRaw.Length; i++)
                _stopTimeOverrides.Add(new StopTimeOverride(stop_time_overridesRaw[i]));

            // Realtime GTFS
            _feedRealtime = Serializer.Deserialize<FeedMessage>(new FileStream("GTFS/c21ac6758dd25af84cca5b707f3cb3de", FileMode.Open, FileAccess.Read));

            // Update ListViews
            UpdateLv(SelectedLv.BeginStationRouteplanner);
            UpdateLv(SelectedLv.EndStationRouteplanner);
            UpdateLv(SelectedLv.StationTripviewer);
            UpdateLv(SelectedLv.StationRealtime);
            UpdateLv(SelectedLv.StationRoutefinder);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-BE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("nl-BE");
        }



        //// METHODS

        /// <summary>
        /// Returns the parent station as a 'Stop' for the given -Trimed- stopId.
        /// </summary>
        /// <param name="stopId">Trimmed! stopId</param>
        /// <returns>The parent station as 'Stop'</returns>
        private Stop GetStop(string stopId)
        {
            _stops.TryGetValue(stopId, out Stop value);
            return value;

        }
        
        /// <summary>
        /// Returns the Trip for the given tripId.
        /// </summary>
        private Trip GetTrip(string tripId)
        {
            _trips.TryGetValue(tripId, out Trip value);
            return value;
        }

        /// <summary>
        /// Returns the Route for the given routeId.
        /// </summary>
        private Route GetRoute(string routeId)
        {
            _routes.TryGetValue(routeId, out RouteExtra value);
            return value;
        }

        /// <summary>
        /// Returns the Calendar for the given serviceId.
        /// </summary>
        private GTFS.Entities.Calendar GetCalendar(string serviceId)
        {
            _calendars.TryGetValue(serviceId, out GTFS.Entities.Calendar value);
            return value;
        }

        /// <summary>
        /// Returns the Transfer from the Stop for the given -Trimmed- stopId.
        /// </summary>
        /// <param name="fromStopId">Trimmed! stopId</param>
        private Transfer GetTransfer(string fromStopId)
        {
            _transfers.TryGetValue(fromStopId, out Transfer value);
            return value;

        }

        /// <summary>
        /// Returns the Translation for the set LANG.
        /// </summary>
        /// <param name="transId">transId (in French)</param>
        private string GetTrans(string transId)
        {
            if (transId == null)
                return "";
            if (transId == "" || transId == NA)
                return transId;
            _translations.TryGetValue(transId, out Dictionary<string, string> trans);
            string name = transId;
            if (trans != null)
                trans.TryGetValue(_LANG, out name);
            return name;
        }

        /// <summary>
        /// Filter the results in selected ListView based on the input of according TextBox.
        /// </summary>
        /// <param name="selectedLv">The ListView to update.</param>
        private void UpdateLv(SelectedLv selectedLv)
        {
            string tbxStationText = "";
            //// tbxStationText = TextBox.Text.ToLower();
            switch (selectedLv)
            {
                case SelectedLv.BeginStationRouteplanner: tbxStationText = tbxBeginStationRouteplanner.Text.ToLower(); break;
                case SelectedLv.EndStationRouteplanner: tbxStationText = tbxEndStationRouteplanner.Text.ToLower(); break;
                case SelectedLv.StationTripviewer: tbxStationText = tbxStationTripviewer.Text.ToLower(); break;
                case SelectedLv.StationRealtime: tbxStationText = tbxStationRealtime.Text.ToLower(); break;
                case SelectedLv.StationRoutefinder: tbxStationText = tbxStationRoutefinder.Text.ToLower(); break;
            }

            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && GetTrans(station.Value.Name).ToLower().Contains(tbxStationText)
                                  orderby GetTrans(station.Value.Name) ascending
                                  select station.Value;

            List<Tuple<string, Stop>> itemSource = new List<Tuple<string, Stop>>();

            foreach (Stop s in filterdStations)
            {
                itemSource.Add(Tuple.Create(GetTrans(s.Name), s));
            }

            //// ListView.ItemsSource = itemSource;
            switch (selectedLv)
            {
                case SelectedLv.BeginStationRouteplanner: lvBeginStationRouteplanner.ItemsSource = itemSource; break;
                case SelectedLv.EndStationRouteplanner: lvEndStationRouteplanner.ItemsSource = itemSource; break;
                case SelectedLv.StationTripviewer: lvStationTripviewer.ItemsSource = itemSource; break;
                case SelectedLv.StationRealtime: lvStationRealtime.ItemsSource = itemSource; break;
                case SelectedLv.StationRoutefinder: lvStationRoutefinder.ItemsSource = itemSource; break;
            }
        }



        //// EVENT HANDLERS

        /// <summary>
        /// Initial setting the DisplayDateStart and DisplayDateEnd for dpRouteplanner and dpTripviewer.
        /// </summary>
        /// <param name="sender">dpRouteplanner or dpTripviewer</param>
        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            dpRouteplanner.DisplayDateStart = dpTripviewer.DisplayDateStart = _feedStatic.Calendars.First().StartDate;
            dpRouteplanner.DisplayDateEnd = dpTripviewer.DisplayDateEnd = _feedStatic.Calendars.First().EndDate;
            dpRouteplanner.SelectedDate = dpTripviewer.SelectedDate = DateTime.Now;
            dpRouteplanner.Text = dpTripviewer.Text = DateTime.Now.ToLongDateString();
        }

        /// <summary>
        /// Initial setting the displayed Value for tpRouteplanner.
        /// </summary>
        /// <param name="sender">tpRouteplanner</param>
        private void TimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            tpRouteplanner.Value = DateTime.Now;
        }



        //////// ROUTE PLANNER

        /// <summary>
        /// Call method to update lvBeginStationRouteplanner.
        /// </summary>
        /// <param name="sender">tbxBeginStationRouteplanner</param>
        private void TbxBeginStationRouteplanner_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLv(SelectedLv.BeginStationRouteplanner);
        }

        /// <summary>
        /// Catch the DoubleClick event and sets _searchBeginStation to the SelectedItem.
        /// </summary>
        /// <param name="sender">lvBeginStationRouteplanner</param>
        private void LvBeginStationRouteplanner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                _searchBeginStationRouteplanner = lv.ItemsSource.Cast<Tuple<string, Stop>>().ElementAt(lv.SelectedIndex).Item2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchBeginStationRouteplanner != null)
                tbxBeginStationRouteplanner.Text = GetTrans(_searchBeginStationRouteplanner.Name);

            if (_searchEndStationRoutePlanner != null)
                btnQueryRouteplanner.IsEnabled = true;
            else
                btnQueryRouteplanner.IsEnabled = false;
        }

        /// <summary>
        /// Call method to update lvEndStationRouteplanner.
        /// </summary>
        /// <param name="sender">tbxEndStationRouteplanner</param>
        private void TbxEndStationRouteplanner_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLv(SelectedLv.EndStationRouteplanner);
        }

        /// <summary>
        /// Catch the DoubleClick event and set _searchEndStation to the SelectedItem.
        /// </summary>
        /// <param name="sender">lvEndStationRouteplanner</param>
        private void LvEndStationRouteplanner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                _searchEndStationRoutePlanner = lv.ItemsSource.Cast<Tuple<string, Stop>>().ElementAt(lv.SelectedIndex).Item2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchEndStationRoutePlanner != null)
                tbxEndStationRouteplanner.Text = GetTrans(_searchEndStationRoutePlanner.Name);

            if (_searchBeginStationRouteplanner != null)
                btnQueryRouteplanner.IsEnabled = true;
            else
                btnQueryRouteplanner.IsEnabled = false;
        }

        /// <summary>
        /// Filter based on _searchBeginStation, _searchEndStation, the selected date and time.
        /// Display the results in lvResult.
        /// </summary>
        /// <param name="sender">btnQueryRouteplanner</param>
        private void BtnQueryRouteplanner_Click(object sender, RoutedEventArgs e)
        {
            Stop stop = GetStop(_searchBeginStationRouteplanner.Id.Trimmed());

            List<Tuple<string, Stop>> itemSource = new List<Tuple<string, Stop>>();

            _sb.Clear()
                .Append('[')
                .Append(stop.Id)
                .Append(']')
                .Append(WS)
                .Append(GetTrans(stop.Name));

            itemSource.Add(Tuple.Create(_sb.ToString(), stop));

            lvResultRouteplanner.ItemsSource = itemSource;
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvResultRouteplanner</param>
        private void LvResultRouteplanner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /////////////////// GRAND FINALE

            ////// met routeId uit Trip de Route zoeken en StopList eruit halen
            ////// voor iedere Stop in StopList, de StopTime zoeken ahv stopId, waar tripId = tripId van de Trip
            ////// Zorgen dan StopSequence eindstation > beginstation
            ////// ArrivalTime, StopTime, StopName, StopSequence nemen
            ////// Nieuwe window open doen
            ////// Listview met StopSequence, StopName, ArrivalTime en DepartureTime
            //////
            ////// Dan enkel de info tonen tussen begin en eindstation
            ////// Dan enkel eerste 3 hits tonen


        }



        //////// TRIP VIEWER

        /// <summary>
        /// Call method to update lvStationTripviewer.
        /// </summary>
        /// <param name="sender">tbxStationTripviewer</param>
        private void TbxStationTripviewer_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLv(SelectedLv.StationTripviewer);
        }

        /// <summary>
        /// Catch the DoubleClick event and set _searchTripStation.
        /// </summary>
        /// <param name="sender">lvStationTripviewer</param>
        private void LvStationTripviewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                _searchStationTripviewer = lv.ItemsSource.Cast<Tuple<string, Stop>>().ElementAt(lv.SelectedIndex).Item2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchStationTripviewer != null)
            {
                btnQueryTripviewer.IsEnabled = true;
                tbxStationTripviewer.Text = GetTrans(_searchStationTripviewer.Name);
            }
            else
                btnQueryTripviewer.IsEnabled = false;
        }

        /// <summary>
        /// Filter based on _searchTripStation and the selected date.
        /// Display the results in lvTrips.
        /// </summary>
        /// <param name="sender">btnQueryTripviewer</param>
        private void BtnQueryTripviewer_Click(object sender, RoutedEventArgs e)
        {
            var stops = _feedStatic.StopTimes
                .GetForStop(_searchStationTripviewer.Id.Trimmed())
                .Where(x => DateTime.ParseExact(x.TripId.Split(':')[7], "yyyyMMdd", new CultureInfo("fr-FR")) > DateTime.Now);

            List<Tuple<string>> itemSource = new List<Tuple<string>>();

            foreach (StopTime stopTime in stops)
            {
                DateTime dateDT = DateTime
                    .ParseExact(stopTime.TripId.Split(':')[7], "yyyyMMdd", new CultureInfo("fr-FR"))
                    .AddHours(Convert.ToDouble(stopTime.DepartureTime.Hours))
                    .AddMinutes(Convert.ToDouble(stopTime.DepartureTime.Minutes));

                if (stopTime.TripId.Split(':')[4] != _searchStationTripviewer.Id.Trimmed())
                {
                    Stop stopToForName = GetStop(stopTime.TripId.Split(':')[4]);
                    _sb.Clear()
                        .Append(dateDT.ToShortDateString())
                        .Append('\t')
                        .AppendLine(dateDT.ToLongTimeString())
                        .Append("DEPARTURE: Train to ")
                        .Append(GetTrans(stopToForName.Name));
                }
                else
                {
                    Stop stopToForName = GetStop(stopTime.TripId.Split(':')[3]);
                    _sb.Clear()
                        .Append(dateDT.ToShortDateString())
                        .Append('\t')
                        .AppendLine(dateDT.ToLongTimeString())
                        .Append("ARRIVAL: Train from ")
                        .Append(GetTrans(stopToForName.Name));
                }

                itemSource.Add(Tuple.Create(_sb.ToString()));
            }

            lvResultTripviewer.ItemsSource = itemSource;
        }



        //////// REAL TIME

        /// <summary>
        /// Call method to update lvStationRealtime.
        /// </summary>
        /// <param name="sender">tbxStationRealtime</param>
        private void TbxStationRealtime_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLv(SelectedLv.StationRealtime);
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// <para/>
        /// Fills lvResultRealtime.
        /// </summary>
        /// <param name="sender">lvStationRealtime</param>
        private void LvStationRealtime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                _searchStationRealtime = lv.ItemsSource.Cast<Tuple<string, Stop>>().ElementAt(lv.SelectedIndex).Item2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchStationRealtime != null)
            {
                tbxStationRealtime.Text = GetTrans(_searchStationRealtime.Name);

                var filterdEntities = from entity in _feedRealtime.entity
                                      where entity.id.Split(':')[3] == _searchStationRealtime.Id.Trimmed() || entity.id.Split(':')[4] == _searchStationRealtime.Id.Trimmed()
                                      select entity;

                List<Tuple<string, string, Tuple<string, string, string>[]>> itemSource = new List<Tuple<string, string, Tuple<string, string, string>[]>>();

                foreach (FeedEntity entity in filterdEntities)
                {
                    _stops.TryGetValue(entity.id.Split(':')[3], out Stop stopTemp1);

                    _stops.TryGetValue(entity.id.Split(':')[4], out Stop stopTemp2);

                    _sb.Clear()
                        .Append(GetTrans(stopTemp1?.Name ?? NA))
                        .Append(" -> ")
                        .AppendLine(GetTrans(stopTemp2?.Name ?? NA))
                        .Append(DateTime.ParseExact(entity.trip_update.trip.start_date, "yyyyMMdd", new CultureInfo("fr-FR")).ToShortDateString())
                        .Append('\t')
                        .Append(entity.trip_update.trip.start_time);

                    Tuple<string, string, string>[] resultInfo = new Tuple<string, string, string>[entity.trip_update.stop_time_update.Count];
                    int count = 0;
                    foreach (TripUpdate.StopTimeUpdate update in entity.trip_update.stop_time_update)
                    {
                        _stops.TryGetValue(update.stop_id, out Stop stopInStopUpdate);

                        resultInfo[count] = Tuple.Create(
                            GetTrans(stopInStopUpdate?.Name ?? NA),
                            entity.trip_update.stop_time_update[count].arrival?.time != null
                                ? DateTimeOffset.FromUnixTimeSeconds(entity.trip_update.stop_time_update[count].arrival.time).DateTime.ToLocalTime().ToLongTimeString()
                                : "",
                            entity.trip_update.stop_time_update[count].departure?.time != null
                                ? DateTimeOffset.FromUnixTimeSeconds(entity.trip_update.stop_time_update[count].departure.time).DateTime.ToLocalTime().ToLongTimeString()
                                : "");
                        count++;
                    }

                    itemSource.Add(Tuple.Create(entity.id, _sb.ToString(), resultInfo));
                }

                lvResultRealtime.ItemsSource = itemSource;
            }
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// <para/>
        /// Opens a new window with the specific info for the selected realtime trip update.
        /// </summary>
        /// <param name="sender">lvResultRealtime</param>
        private void LvResultRealtime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            Tuple<string, string, Tuple<string, string, string>[]> resultTemp = null;
            try
            {
                resultTemp = lv.ItemsSource.Cast<Tuple<string, string, Tuple<string, string, string>[]>>().ElementAt(lv.SelectedIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (resultTemp != null)
            {
                ResultRealtime resultRealtime = new ResultRealtime(resultTemp.Item1, resultTemp.Item2, resultTemp.Item3 );
                resultRealtime.Show();
            }
        }



        //////// ROUTE FINDER

        /// <summary>
        /// Call method to update lvStationRoutefinder
        /// </summary>
        /// <param name="sender">tbxStationRoutefinder</param>
        private void TbxStationRoutefinder_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLv(SelectedLv.StationRoutefinder);
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// <para/>
        /// Fills lvResultRoutefinder.
        /// </summary>
        /// <param name="sender">lvStationRoutefinder</param>
        private void LvStationRoutefinder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                _searchStationRoutefinder = lv.ItemsSource.Cast<Tuple<string, Stop>>().ElementAt(lv.SelectedIndex).Item2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if(_searchStationRoutefinder != null)
            {
                tbxStationRoutefinder.Text = GetTrans(_searchStationRoutefinder.Name);

                List<RouteExtra> routes = _routes.Values.Where(x => x.StopList.Keys.Contains(_searchStationRoutefinder.Id.Trimmed())).ToList();

                List<Tuple<string, string, Tuple<string, string, string>[]>> itemSource = new List<Tuple<string, string, Tuple<string, string, string>[]>>();

                foreach (RouteExtra routeExtra in routes)
                {

                    _sb.Clear()
                        .Append(routeExtra.ShortName)
                        .Append('\t')
                        .Append(GetTrans(routeExtra.LongName.Split(RLNS, StringSplitOptions.None)[0].TrimEnd(WS)))
                        .Append(WS)
                        .Append('-', 2)
                        .Append(WS)
                        .Append(GetTrans(routeExtra.LongName.Split(RLNS, StringSplitOptions.None)[1].TrimStart(WS)));

                    Tuple<string, string, string>[] resultInfo = new Tuple<string, string, string>[routeExtra.StopList.Count];
                    int count = 0;
                    foreach (Stop s in routeExtra.StopList.Values)
                    {
                        resultInfo[count++] = Tuple.Create(
                            GetTrans(s.Name),
                            s.Latitude.ToString(),
                            s.Longitude.ToString());
                    }

                    itemSource.Add(Tuple.Create(routeExtra.Id, _sb.ToString(), resultInfo));
                }

                lvResultRoutefinder.ItemsSource = itemSource;
            }
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvResultRoutefinder</param>
        private void LvResultRoutefinder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            Tuple<string, string, Tuple<string, string, string>[]> resultTemp = null;
            try
            {
                resultTemp = lv.ItemsSource.Cast<Tuple<string, string, Tuple<string, string, string>[]>>().ElementAt(lv.SelectedIndex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (resultTemp != null)
            {
                ResultRoutefinder resultRoutefinder = new ResultRoutefinder(resultTemp.Item1, resultTemp.Item2, resultTemp.Item3);
                resultRoutefinder.Show();
            }
        }
    }
}
