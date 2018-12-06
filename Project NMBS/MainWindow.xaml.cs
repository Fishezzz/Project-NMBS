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

        //List<Stop> _stops;
        //List<Trip/*Extra*/> _trips;
        //List<Route/*Extra*/> _routes;
        //List<Agency> _agencies;
        //List<GTFS.Entities.Calendar> _calendars;
        //List<CalendarDate> _calendarDates;
        //List<StopTime> _stopTimes;
        //List<StopTimeOverride> _stopTimeOverrides;
        //List<Transfer> _transfers;
        //List<Translation> _translations;

        public static Dictionary<string, Stop> _stops;
        Dictionary<string, Trip> _trips;
        Dictionary<string, Route> _routes;
        Dictionary<string, Agency> _agencies;
        Dictionary<string, GTFS.Entities.Calendar> _calendars;
        Dictionary<string, CalendarDate> _calendarDates;
        Dictionary<string, StopTime> _stopTimes;
        /// <summary>
        /// <param name="T1">TripId</param>
        /// <param name="T2">ServiceId</param>
        /// <param name="T3">StopId</param>
        /// </summary>
        Dictionary<Tuple<string, string, string>, StopTimeOverride> _stopTimeOverrides;
        Dictionary<string, Transfer> _transfers;
        Dictionary<string, Translation> _translations;

        Stop _searchBeginStationRouteplanner;
        Stop _searchEndstationPlanner;
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
            //_calendarDates = _feedStatic.CalendarDates.ToDictionary(x => x.ServiceId, x => x);
            //_stopTimes = _feedStatic.StopTimes.ToDictionary(x => x.TripId, x => x);
            _stopTimeOverrides = new Dictionary<Tuple<string, string, string>, StopTimeOverride>();
            _transfers = _feedStatic.Transfers.ToDictionary(x => x.FromStopId, x => x);
            _translations = new Dictionary<string, Translation>();

            // Read extra files and create lists with objects
            //string[] stop_time_overridesRaw = File.ReadAllLines("GTFS\\stop_time_overrides.txt");
            //for (int i = 1; i < stop_time_overridesRaw.Length; i++)
            //    _stopTimeOverrides.Add(
            //        Tuple.Create(
            //            new StopTimeOverride(stop_time_overridesRaw[i]).Trip_Id,
            //            new StopTimeOverride(stop_time_overridesRaw[i]).Service_Id,
            //            new StopTimeOverride(stop_time_overridesRaw[i]).Stop_Id),
            //        new StopTimeOverride(stop_time_overridesRaw[i]));
            //string[] translationsRaw = File.ReadAllLines("GTFS\\translations.txt");
            //for (int i = 1; i < translationsRaw.Length; i++)
            //    _translations.Add(
            //        new Translation(translationsRaw[i]).Trans_Id,
            //        new Translation(translationsRaw[i]));

            // Update ListViews
            UpdateLvBeginStationRouteplanner();
            UpdateLvEndStationRouteplanner();
            UpdateLvResultTripviewer();
            UpdateLvStationRealtime();

            // Realtime GTFS
            _feedRealtime = Serializer.Deserialize<FeedMessage>(new FileStream("GTFS/realtime", FileMode.Open, FileAccess.Read));
        }

        //// METHODS
        
            //// TODO: Onderstaande methods tesamen voegen + 

        /// <summary>
        /// Filter the results in lvBeginStationRouteplanner based on the input of tbxBeginStationRouteplanner.
        /// </summary>
        private void UpdateLvBeginStationRouteplanner()
        {
            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && station.Value.Name.ToLower().Contains(tbxBeginStationRouteplanner.Text.ToLower())
                                  orderby station.Value.Name ascending
                                  select station;

            lvBeginStationRouteplanner.Items.Clear();

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Value.Name;
                lbi.Tag = s.Value;
                lvBeginStationRouteplanner.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Filter the results in lvEndStationRouteplanner based on the input of tbxEndStationRouteplanner.
        /// </summary>
        private void UpdateLvEndStationRouteplanner()
        {
            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && station.Value.Name.ToLower().Contains(tbxEndStationRouteplanner.Text.ToLower())
                                  orderby station.Value.Name ascending
                                  select station;

            lvEndStationRouteplanner.Items.Clear();

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Value.Name;
                lbi.Tag = s.Value;
                lvEndStationRouteplanner.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Filter the results in lvStationTripviewer based on the input of tbxStationTripviewer.
        /// </summary>
        private void UpdateLvResultTripviewer()
        {
            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && station.Value.Name.ToLower().Contains(tbxEndStationRouteplanner.Text.ToLower())
                                  orderby station.Value.Name ascending
                                  select station;

            lvStationTripviewer.Items.Clear();

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Value.Name;
                lbi.Tag = s.Value;
                lvStationTripviewer.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Filter the results in lvStationRealtime based on the input of tbxStationRealtime.
        /// </summary>
        private void UpdateLvStationRealtime()
        {
            var filterdStations = from station in _stops
                                  where station.Value.LocationType == LocationType.Station && station.Value.Name.ToLower().Contains(tbxEndStationRouteplanner.Text.ToLower())
                                  orderby station.Value.Name ascending
                                  select station;

            lvStationRealtime.Items.Clear();

            foreach (KeyValuePair<string, Stop> s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Value.Name;
                lbi.Tag = s.Value;
                lvStationRealtime.Items.Add(lbi);
            }
        }



        //// EVENT HANDLERS

        //////// ROUTE PLANNER

        /// <summary>
        /// Initial setting the DisplayDateStart and DisplayDateEnd for dpRouteplanner and dpTripviewer.
        /// </summary>
        /// <param name="sender">dpRouteplanner or dpTripviewer</param>
        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            dpRouteplanner.DisplayDateStart = dpTripviewer.DisplayDateStart = _feedStatic.Calendars.First().StartDate;
            dpRouteplanner.DisplayDateEnd = dpTripviewer.DisplayDateEnd = _feedStatic.Calendars.First().EndDate;
        }

        /// <summary>
        /// Initial setting the displayed Value for tpRouteplanner.
        /// </summary>
        /// <param name="sender">tpRouteplanner</param>
        private void TimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            tpRouteplanner.Value = DateTime.Now;
        }


        /// <summary>
        /// Call method to update lvBeginStationRouteplanner.
        /// </summary>
        /// <param name="sender">tbxBeginStationRouteplanner</param>
        private void TbxBeginStationRouteplanner_TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateLvBeginStationRouteplanner();
        }

        /// <summary>
        /// Catche the DoubleClick event and sets _searchBeginStation to the SelectedItem.
        /// </summary>
        /// <param name="sender">lvBeginStationRouteplanner</param>
        private void LvBeginStationRouteplanner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ListView lv = (ListView)sender;
            //try
            //{
            //    ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
            //    _searchBeginStationRouteplanner = (Stop)lbi.Tag;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

            //if (_searchEndstationPlanner != null)
            //{
            //    btnQueryRouteplanner.IsEnabled = true;
            //    tbxBeginStationRouteplanner.Text = _searchBeginStationRouteplanner.Name;
            //}
            //else
            //    btnQueryRouteplanner.IsEnabled = false;
        }

        /// <summary>
        /// Call method to update lvEndStationRouteplanner.
        /// </summary>
        /// <param name="sender">tbxEndStationRouteplanner</param>
        private void TbxEndStationRouteplanner_TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateLvEndStationRouteplanner();
        }

        /// <summary>
        /// Cath the DoubleClick event and set _searchEndStation to the SelectedItem.
        /// </summary>
        /// <param name="sender">lvEndStationRouteplanner</param>
        private void LvEndStationRouteplanner_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ListView lv = (ListView)sender;
            //try
            //{
            //    ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
            //    _searchEndstationPlanner = (Stop)lbi.Tag;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

            //if (_searchBeginStationRouteplanner != null)
            //{
            //    btnQueryRouteplanner.IsEnabled = true;
            //    tbxEndStationRouteplanner.Text = _searchEndstationPlanner.Name;
            //}
            //else
            //    btnQueryRouteplanner.IsEnabled = false;
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
            //var gefilterdeStations = from station in _stops
            //                         where station.Key == _searchBeginStationRouteplanner.Id
            //                         select station;

            //lvResultRouteplanner.Items.Clear();

            //foreach (KeyValuePair<string, Stop> s in gefilterdeStations)
            //{
            //    ListBoxItem lbi = new ListBoxItem();
            //    lbi.Content = $"[{s.Key}] {s.Value.Name}";
            //    lbi.Tag = s.Value;
            //    lvResultRouteplanner.Items.Add(lbi);
            //}
        }



        //////// TRIP VIEWER

        /// <summary>
        /// Call method to update lvStationTripviewer.
        /// </summary>
        /// <param name="sender">tbxStationTripviewer</param>
        private void TbxStationTripviewer_TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateLvResultTripviewer();
        }

        /// <summary>
        /// Catch the DoubleClick event and set _searchTripStation.
        /// </summary>
        /// <param name="sender">lvStationTripviewer</param>
        private void LvStationTripviewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ListView lv = (ListView)sender;
            //try
            //{
            //    ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
            //    _searchStationTripviewer = (Stop)lbi.Tag;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

            //if (_searchStationTripviewer != null)
            //{
            //    btnQueryTripviewer.IsEnabled = true;
            //    tbxStationTripviewer.Text = _searchStationTripviewer.Name;
            //}
            //else
            //    btnQueryTripviewer.IsEnabled = false;
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
            //var gefilterdeTrips = from trip in _stopTimes
            //                      where trip.Key == _searchStationTripviewer.Id.TrimStart('S').Split('_')[0]
            //                      orderby trip.Value.TripId.Split(':')[7] ascending
            //                      select trip;

            //lvResultTripviewer.Items.Clear();
            
            //foreach (KeyValuePair<string, StopTime> stopTime in gefilterdeTrips)
            //{
            //    ListBoxItem lbi = new ListBoxItem();
            //    var stopName = from stop in _stops where stop.Key.TrimStart('S') == stopTime.Key.Split(':')[4] select stop;
            //    DateTime dateDT = DateTime.ParseExact(stopTime.Key.Split(':')[7], "yyyyMMdd", new CultureInfo("fr-FR")).AddHours(Convert.ToDouble(stopTime.Value.DepartureTime.Hours)).AddMinutes(Convert.ToDouble(stopTime.Value.DepartureTime.Minutes));
            //    lbi.Content = $"[{stopTime.Key}]\n{dateDT}\n Train to {stopName.First().Value.Name}";
            //    lbi.Tag = stopTime;
            //    lvResultTripviewer.Items.Add(lbi);
            //}
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
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvStationRealtime</param>
        private void LvStationRealtime_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ListView lv = (ListView)sender;
            //try
            //{
            //    ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
            //    _searchStationRealtime = (Stop)lbi.Tag;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

            //if (_searchStationRealtime != null)
            //{
            //    tbxStationRealtime.Text = _searchStationRealtime.Name;
            //    // [3]
            //    // [4]
            //    var filterdEntities = from entity in _feedRealtime.entity
            //                          where entity.id.Split(':')[3] == _searchStationRealtime.Id.TrimStart('S').Split('_')[0]
            //                          select entity;

            //    lvResultRealtime.Items.Clear();

            //    foreach (FeedEntity entity in filterdEntities)
            //    {
            //        ListBoxItem lbi = new ListBoxItem();
            //        DateTime dt = new DateTime();
            //        lbi.Content = $"Arrival: {dt.AddSeconds(entity.trip_update.stop_time_update.Last().arrival.time).ToShortTimeString()}";
            //        //lbi.Content = $"{entity.trip_update.stop_time_update.Last().arrival}\t{entity.trip_update.stop_time_update.Last().departure}";
            //        lbi.Tag = entity;
            //        lvResultRealtime.Items.Add(lbi);
            //    }
            //}
        }

        private void ExpStationRealtime_Expanded(object sender, RoutedEventArgs e)
        {
            //expStationRealtime.Height = 391;
        }

        private void ExpStationRealtime_Collapsed(object sender, RoutedEventArgs e)
        {
            //expStationRealtime.Height = 30;
        }
    }
}
