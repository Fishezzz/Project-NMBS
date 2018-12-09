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
        GTFSFeed _feedStatic;
        FeedMessage _feedRealtime;

        ////List<Stop> _stops;
        ////List<Trip/*Extra*/> _trips;
        ////List<Route/*Extra*/> _routes;
        ////List<Agency> _agencies;
        ////List<GTFS.Entities.Calendar> _calendars;
        ////Dictionary<string, CalendarDate> _calendarDates;
        ////List<StopTime> _stopTimes;
        ////Dictionary<Tuple<string, string, string>, StopTimeOverride> _stopTimeOverrides;
        ////List<Transfer> _transfers;
        ////List<Translation> _translations;

        /// <summary>
        /// Key: Stop_Id
        /// <para/>
        /// Value: Stop
        /// </summary>
        public static Dictionary<string, Stop> _stops;
        /// <summary>
        /// Key: Trip_Id
        /// <para/>
        /// Value: Trip
        /// </summary>
        Dictionary<string, Trip> _trips;
        /// <summary>
        /// Key: Route_Id
        /// <para/>
        /// Value: Route
        /// </summary>
        Dictionary<string, Route> _routes;
        /// <summary>
        /// Key: Agency_Id
        /// <para/>
        /// Value: Agency
        /// </summary>
        Dictionary<string, Agency> _agencies;
        /// <summary>
        /// Key: Service_Id
        /// <para/>
        /// Value: Calendar
        /// </summary>
        Dictionary<string, GTFS.Entities.Calendar> _calendars;
        /// <summary>
        /// Key: From_Stop_Id
        /// <para/>
        /// Value: Transfer
        /// </summary>
        Dictionary<string, Transfer> _transfers;

        /// <summary>
        /// Key-T1: Service_Id
        /// <para/>
        /// Key-T2: Date
        /// <para/>
        /// Value: Exception_Type
        /// </summary>
        Dictionary<Tuple<string, DateTime>, ExceptionType> _calendarDates;
        /// <summary>
        /// Key-T1: Trip_Id
        /// <para/>
        /// Key-T2: Stop_Sequence
        /// <para/>
        /// Value: StopTime
        /// </summary>
        Dictionary<Tuple<string, uint>, StopTime> _stopTimes;
        /// <summary>
        /// Key: Trans_Id
        /// <para/>
        /// Value-Key: Language
        /// <para/>
        /// Value-Value: Trans
        /// </summary>
        Dictionary<string, Dictionary<string, string>> _translations;
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

            // Create lists with objects
            _stops = _feedStatic.Stops.ToDictionary(x => x.Id, x => x);
            _trips = _feedStatic.Trips.ToDictionary(x => x.Id, x => x);     /*.ToTripExtraList();*/
            _routes = _feedStatic.Routes.ToDictionary(x => x.Id, x => x);   /*.ToRouteExtraList();*/
            _agencies = _feedStatic.Agencies.ToDictionary(x => x.Id, x => x);
            _calendars = _feedStatic.Calendars.ToDictionary(x => x.ServiceId, x => x);
            _transfers = _feedStatic.Transfers.ToDictionary(x => x.FromStopId, x => x);

            _translations = new Dictionary<string, Dictionary<string, string>>();
            _stopTimeOverrides = new List<StopTimeOverride>();

            _calendarDates = _feedStatic.CalendarDates.ToDictionary(x => Tuple.Create(x.ServiceId, x.Date), x => x.ExceptionType);
            _stopTimes = _feedStatic.StopTimes.ToDictionary(x => Tuple.Create(x.TripId, x.StopSequence), x => x);

            // Read extra files and create lists with objects
            string[] translationsRaw = File.ReadAllLines("GTFS\\translations.txt");
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
            string[] stop_time_overridesRaw = File.ReadAllLines("GTFS\\stop_time_overrides.txt");
            for (int i = 1; i < stop_time_overridesRaw.Length; i++)
                _stopTimeOverrides.Add(new StopTimeOverride(stop_time_overridesRaw[i]));

            // Update ListViews
            //UpdateLvBeginStationRouteplanner();
            //UpdateLvEndStationRouteplanner();
            //UpdateLvResultTripviewer();
            //UpdateLvStationRealtime();
            UpdateLv(SelectedLv.BeginStationRouteplanner);
            UpdateLv(SelectedLv.EndStationRouteplanner);
            UpdateLv(SelectedLv.ResultTripviewer);
            UpdateLv(SelectedLv.StationRealtime);

            // Realtime GTFS
            _feedRealtime = Serializer.Deserialize<FeedMessage>(new FileStream("GTFS/realtime", FileMode.Open, FileAccess.Read));
        }

        //// METHODS

        /// <summary>
        /// Filter the results in selected ListView based on the input of according TextBox.
        /// </summary>
        /// <param name="selectedLv">The ListView to update.</param>
        private void UpdateLv(SelectedLv selectedLv)
        {
            string tbxStationText = "";
            switch (selectedLv)
            {
                case SelectedLv.BeginStationRouteplanner: tbxStationText = tbxBeginStationRouteplanner.Text.ToLower(); break;
                case SelectedLv.EndStationRouteplanner: tbxStationText = tbxEndStationRouteplanner.Text.ToLower(); break;
                case SelectedLv.ResultTripviewer: tbxStationText = tbxStationTripviewer.Text.ToLower(); break;
                case SelectedLv.StationRealtime: tbxStationText = tbxStationRealtime.Text.ToLower(); break;
            }

            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && station.Value.Name.ToLower().Contains(tbxStationText)
                                  orderby station.Value.Name ascending
                                  select station;

            // ListView.Items.Clear();
            switch (selectedLv)
            {
                case SelectedLv.BeginStationRouteplanner: lvBeginStationRouteplanner.Items.Clear(); break;
                case SelectedLv.EndStationRouteplanner: lvEndStationRouteplanner.Items.Clear(); break;
                case SelectedLv.ResultTripviewer: lvStationTripviewer.Items.Clear(); break;
                case SelectedLv.StationRealtime: lvStationRealtime.Items.Clear(); break;
            }

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem
                {
                    Content = s.Value.Name,
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
            //UpdateLvBeginStationRouteplanner();
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

            tbxBeginStationRouteplanner.Text = _searchBeginStationRouteplanner.Name ?? "";

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
            //UpdateLvEndStationRouteplanner();
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

            tbxEndStationRouteplanner.Text = _searchEndStationRoutePlanner.Name ?? "";

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
            var gefilterdeStations = from station in _stops
                                     where station.Key == _searchBeginStationRouteplanner.Id
                                     select station;

            lvResultRouteplanner.Items.Clear();

            foreach (KeyValuePair<string, Stop> s in gefilterdeStations)
            {
                ListBoxItem lbi = new ListBoxItem
                {
                    Content = $"[{s.Key}] {s.Value.Name}",
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
            //UpdateLvResultTripviewer();
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
                tbxStationTripviewer.Text = _searchStationTripviewer.Name;
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
            var gefilterdeTrips = from trip in _stopTimes
                                  where trip.Key.Item1.Split(':')[3] == _searchStationTripviewer.Id.TrimStart('S').Split('_')[0]
                                  orderby trip.Value.TripId.Split(':')[7] ascending
                                  select trip;

            lvResultTripviewer.Items.Clear();

            foreach (KeyValuePair<Tuple<string, uint>, StopTime> stopTime in gefilterdeTrips)
            {
                var stopName = from stop in _stops where stop.Key.TrimStart('S') == stopTime.Key.Item1.Split(':')[4] select stop;
                DateTime dateDT = DateTime
                    .ParseExact(stopTime.Key.Item1.Split(':')[7], "yyyyMMdd", new CultureInfo("fr-FR"))
                    .AddHours(Convert.ToDouble(stopTime.Value.DepartureTime.Hours))
                    .AddMinutes(Convert.ToDouble(stopTime.Value.DepartureTime.Minutes));

                ListBoxItem lbi = new ListBoxItem
                {
                    Content = $"[{stopTime.Key}]\n{dateDT}\n Train to {stopName.First().Value.Name}",
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
            //UpdateLvStationRealtime();
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
                tbxStationRealtime.Text = _searchStationRealtime.Name;
                var filterdEntities = from entity in _feedRealtime.entity
                                      where entity.id.Split(':')[3] == _searchStationRealtime.Id.TrimStart('S').Split('_')[0]
                                      select entity;

                lvResultRealtime.Items.Clear();

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
