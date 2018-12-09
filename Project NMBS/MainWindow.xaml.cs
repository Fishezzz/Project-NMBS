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
        GTFSFeed _feedStatic;
        FeedMessage _feedRealtime;

        //  fr      nl      de      en
        const string _LANG = "nl";

        ////List<Stop> _stops;
        ////List<Trip> _trips;
        ////List<Route> _routes;
        ////List<Agency> _agencies;
        ////List<GTFS.Entities.Calendar> _calendars;
        ////Dictionary<string, CalendarDate> _calendarDates;
        ////List<StopTime> _stopTimes;
        ////Dictionary<Tuple<string, string, string>, StopTimeOverride> _stopTimeOverrides;
        ////List<Transfer> _transfers;
        ////List<Translation> _translations;

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
        Dictionary<string, Trip> _trips;
        /// <summary>
        /// Key: Route_Id (string)
        /// <para/>
        /// Value: Route (Route)
        /// </summary>
        Dictionary<string, Route> _routes;
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

        enum SelectedLv { BeginStationRouteplanner = 1, EndStationRouteplanner = 2, ResultTripviewer = 3, StationRealtime = 4 }

        public MainWindow()
        {
            InitializeComponent();

            // Read GTFS Static files
            _reader = new GTFSReader<GTFSFeed>(strict: false);
            _feedStatic = _reader.Read(new DirectoryInfo("GTFS"));

            // Read extra files and create lists with objects
            string[] translationsRaw = File.ReadAllLines("GTFS\\translations.txt");
            string[] stop_time_overridesRaw = File.ReadAllLines("GTFS\\stop_time_overrides.txt");

            // Create lists with objects
            _stops = _feedStatic.Stops.ToDictionary(x => x.Id, x => x);
            _trips = _feedStatic.Trips.ToDictionary(x => x.Id, x => x);
            _routes = _feedStatic.Routes.ToDictionary(x => x.Id, x => x);
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

            // Update ListViews
            UpdateLv(SelectedLv.BeginStationRouteplanner);
            UpdateLv(SelectedLv.EndStationRouteplanner);
            UpdateLv(SelectedLv.ResultTripviewer);
            UpdateLv(SelectedLv.StationRealtime);

            // Realtime GTFS
            _feedRealtime = Serializer.Deserialize<FeedMessage>(new FileStream("GTFS/realtime", FileMode.Open, FileAccess.Read));
        }

        //// METHODS

        /// <summary>
        /// Returns the parent station as a 'Stop' for the given -Trimed- stopId.
        /// </summary>
        /// <param name="stopId">Trimmed! stopId</param>
        /// <returns>The parent station as 'Stop'</returns>
        private Stop GetStop(string stopId)
        {
            Stop value;
            if (_stops.TryGetValue("S" + stopId, out value))
                return value;
            else
            {
                _stops.TryGetValue(stopId, out value);
                return value;
            }

        }
        
        /// <summary>
        /// Returns the Trip for the given tripId.
        /// </summary>
        private Trip GetTrip(string tripId)
        {
            Trip value;
            _trips.TryGetValue(tripId, out value);
            return value;
        }

        /// <summary>
        /// Returns the Route for the given routeId.
        /// </summary>
        private Route GetRoute(string routeId)
        {
            Route value;
            _routes.TryGetValue(routeId, out value);
            return value;
        }

        /// <summary>
        /// Returns the Calendar for the given serviceId.
        /// </summary>
        private GTFS.Entities.Calendar GetCalendar(string serviceId)
        {
            GTFS.Entities.Calendar value;
            _calendars.TryGetValue(serviceId, out value);
            return value;
        }

        /// <summary>
        /// Returns the Transfer from the Stop for the given -Trimmed- stopId.
        /// </summary>
        /// <param name="fromStopId">Trimmed! stopId</param>
        private Transfer GetTransfer(string fromStopId)
        {
            Transfer value;
            _transfers.TryGetValue(fromStopId, out value);
            return value;

        }

        /// <summary>
        /// Returns the Translation for the set LANG.
        /// </summary>
        /// <param name="transId">transId (in French)</param>
        private string GetTrans(string transId)
        {
            Dictionary<string, string> trans;
            _translations.TryGetValue(transId, out trans);
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
            // ListView.Items.Clear();
            switch (selectedLv)
            {
                case SelectedLv.BeginStationRouteplanner: lvBeginStationRouteplanner.Items.Clear(); break;
                case SelectedLv.EndStationRouteplanner: lvEndStationRouteplanner.Items.Clear(); break;
                case SelectedLv.ResultTripviewer: lvStationTripviewer.Items.Clear(); break;
                case SelectedLv.StationRealtime: lvStationRealtime.Items.Clear(); break;
            }

            string tbxStationText = "";
            switch (selectedLv)
            {
                case SelectedLv.BeginStationRouteplanner: tbxStationText = tbxBeginStationRouteplanner.Text.ToLower(); break;
                case SelectedLv.EndStationRouteplanner: tbxStationText = tbxEndStationRouteplanner.Text.ToLower(); break;
                case SelectedLv.ResultTripviewer: tbxStationText = tbxStationTripviewer.Text.ToLower(); break;
                case SelectedLv.StationRealtime: tbxStationText = tbxStationRealtime.Text.ToLower(); break;
            }

            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && GetTrans(station.Value.Name).ToLower().Contains(tbxStationText)
                                  orderby GetTrans(station.Value.Name) ascending
                                  select station;

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                Dictionary<string, string> trans;
                _translations.TryGetValue(s.Value.Name, out trans);
                string name;
                trans.TryGetValue(_LANG, out name);
                ListBoxItem lbi = new ListBoxItem
                {
                    Content = /*s.Value.Name*/name,
                    Tag = s.Value
                };

                // ListView.Items.Add(lbi);
                switch (selectedLv)
                {
                    case SelectedLv.BeginStationRouteplanner: lvBeginStationRouteplanner.Items.Add(lbi); break;
                    case SelectedLv.EndStationRouteplanner: lvEndStationRouteplanner.Items.Add(lbi); break;
                    case SelectedLv.ResultTripviewer: lvStationTripviewer.Items.Add(lbi); break;
                    case SelectedLv.StationRealtime: lvStationRealtime.Items.Add(lbi); break;
                }
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
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchBeginStationRouteplanner = (Stop)lbi.Tag;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchBeginStationRouteplanner != null)
                tbxBeginStationRouteplanner.Text = GetTrans(_searchBeginStationRouteplanner.Name) ?? "";

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
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchEndStationRoutePlanner = (Stop)lbi.Tag;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchEndStationRoutePlanner != null)
                tbxEndStationRouteplanner.Text = GetTrans(_searchEndStationRoutePlanner.Name) ?? "";

            if (_searchBeginStationRouteplanner != null)
                btnQueryRouteplanner.IsEnabled = true;
            else
                btnQueryRouteplanner.IsEnabled = false;
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvResultRouteplanner</param>
        private void LvResultRouteplanner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// Filter based on _searchBeginStation, _searchEndStation, the selected date and time.
        /// Display the results in lvResult.
        /// </summary>
        /// <param name="sender">btnQueryRouteplanner</param>
        private void BtnQueryRouteplanner_Click(object sender, RoutedEventArgs e)
        {
            lvResultRouteplanner.Items.Clear();

            var filterdStations = from station in _stops
                                  where station.Key == _searchBeginStationRouteplanner.Id
                                  select station;

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem
                {
                    Content = $"[{s.Key}] {GetTrans(s.Value.Name)}",
                    Tag = s.Value
                };
                lvResultRouteplanner.Items.Add(lbi);
            }
        }



        //////// TRIP VIEWER

        /// <summary>
        /// Call method to update lvStationTripviewer.
        /// </summary>
        /// <param name="sender">tbxStationTripviewer</param>
        private void TbxStationTripviewer_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLv(SelectedLv.ResultTripviewer);
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
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchStationTripviewer = (Stop)lbi.Tag;
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
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvResultTripviewer</param>
        private void LvResultTripviewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// Filter based on _searchTripStation and the selected date.
        /// Display the results in lvTrips.
        /// </summary>
        /// <param name="sender">btnQueryTripviewer</param>
        private void BtnQueryTripviewer_Click(object sender, RoutedEventArgs e)
        {
            lvResultTripviewer.Items.Clear();

            var stops = _feedStatic.StopTimes.GetForStop(_searchStationTripviewer.Id.TrimStart('S')) ;

            foreach (StopTime stopTime in stops)
            {
                string content = "";
                DateTime dateDT = DateTime
                    .ParseExact(stopTime.TripId.Split(':')[7], "yyyyMMdd", new CultureInfo("fr-FR"))
                    .AddHours(Convert.ToDouble(stopTime.DepartureTime.Hours))
                    .AddMinutes(Convert.ToDouble(stopTime.DepartureTime.Minutes));

                if (stopTime.TripId.Split(':')[4] != _searchStationTripviewer.Id.TrimStart('S'))
                {
                    Stop stopToForName = GetStop(stopTime.TripId.Split(':')[4]);
                    content = $"[{stopTime.TripId}]\n{dateDT}\nDEPARTURE: Train to {GetTrans(stopToForName.Name)}";
                }
                else /*if (stopTime.TripId.Split(':')[3] != _searchStationTripviewer.Id.TrimStart('S'))*/
                {
                    Stop stopToForName = GetStop(stopTime.TripId.Split(':')[3]);
                    content = $"[{stopTime.TripId}]\n{dateDT}\nARRIVAL: Train from {GetTrans(stopToForName.Name)}";
                }

                ListBoxItem lbi = new ListBoxItem
                {
                    Content = content,
                    Tag = stopTime
                };
                lvResultTripviewer.Items.Add(lbi);
            }
        }



        //////// REAL TIME

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvResultRealtime</param>
        private void LvResultRealtime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

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
        /// </summary>
        /// <param name="sender">lvStationRealtime</param>
        private void LvStationRealtime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchStationRealtime = (Stop)lbi.Tag;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchStationRealtime != null)
            {
                lvResultRealtime.Items.Clear();

                tbxStationRealtime.Text = GetTrans(_searchStationRealtime.Name);

                var filterdEntities = from entity in _feedRealtime.entity
                                      where entity.id.Split(':')[3] == _searchStationRealtime.Id.TrimStart('S').Split('_')[0]
                                      select entity;

                foreach (FeedEntity entity in filterdEntities)
                {
                    ListBoxItem lbi = new ListBoxItem
                    {
                        Content = $"Arrival: {entity.trip_update.stop_time_update.LastOrDefault().arrival?.time.FromUnixTime().ToLongTimeString() ?? ""}\t\tDeparture: {entity.trip_update.stop_time_update.LastOrDefault().departure?.time.FromUnixTime().ToLongTimeString() ?? ""}",
                        Tag = entity
                    };
                    lvResultRealtime.Items.Add(lbi);
                }
            }
        }

        /// <summary>
        /// Catch the Expanded event and sets the Height of expStationRealtime.
        /// </summary>
        /// <param name="sender">expStationRealtime</param>
        private void ExpStationRealtime_Expanded(object sender, RoutedEventArgs e)
        {
            expStationRealtime.Height = 391;
        }

        /// <summary>
        /// Catch the Collapsed event and sets the Height of expStationRealtime.
        /// </summary>
        /// <param name="sender">expStationRealtime</param>
        private void ExpStationRealtime_Collapsed(object sender, RoutedEventArgs e)
        {
            expStationRealtime.Height = 30;
        }
    }
}
