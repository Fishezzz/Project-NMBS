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


namespace Project_NMBS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    { 
        List<Translation> _translations;
        GTFSReader<GTFSFeed> _reader;
        GTFSFeed _feed;

        List<Agency> _agencies;
        List<GTFS.Entities.Calendar> _calendars;
        List<CalendarDate> _calendarDates;
        List<Route/*Extra*/> _routes;
        public static List<Stop> _stops;
        List<StopTime> _stopTimes;
        List<StopTimeOverride> _stopTimeOverrides;
        List<Transfer> _transfers;
        public static List<Trip/*Extra*/> _trips;

        Stop _searchBeginStation;
        Stop _searchEndstation;
        Stop _searchTripStation;

        public MainWindow()
        {
            InitializeComponent();

            _reader = new GTFSReader<GTFSFeed>(strict: false);
            _feed = _reader.Read(new DirectoryInfo("GTFS"));

            _stops = _feed.Stops.ToList();
            _trips = _feed.Trips.ToList()/*.ToTripExtraList()*/;
            _routes = _feed.Routes.ToList()/*.ToRouteExtraList()*/;

            _agencies = _feed.Agencies.ToList();
            _calendars = _feed.Calendars.ToList();
            _calendarDates = _feed.CalendarDates.ToList();
            _stopTimes = _feed.StopTimes.ToList();
            _stopTimeOverrides = new List<StopTimeOverride>();
            _transfers = _feed.Transfers.ToList();
            _translations = new List<Translation>();

            string[] stop_time_overridesRaw = File.ReadAllLines("GTFS\\stop_time_overrides.txt");
            for (int i = 1; i < stop_time_overridesRaw.Length; i++)
                _stopTimeOverrides.Add(new StopTimeOverride(stop_time_overridesRaw[i]));
            string[] translationsRaw = File.ReadAllLines("GTFS\\translations.txt");
            for (int i = 1; i < translationsRaw.Length; i++)
                _translations.Add(new Translation(translationsRaw[i]));

            UpdateLvBeginStation();
            UpdateLvEndStation();
            UpdateLvTrips();
        }



        /// <summary>
        /// Initial setting the DisplayDateStart and DisplayDateEnd for the DatePicker in the "Routeplanner" tab.
        /// </summary>
        /// <param name="sender">dpDatePicker</param>
        private void DpDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            dpDatePicker.DisplayDateStart = dp2.DisplayDateStart = _feed.Calendars.First().StartDate;
            dpDatePicker.DisplayDateEnd = dp2.DisplayDateEnd = _feed.Calendars.First().EndDate;
        }

        /// <summary>
        /// Initial setting the displayed Value for the TimePicker in the "Routeplanner" tab.
        /// </summary>
        /// <param name="sender">tpTimePicker</param>
        private void TpTimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            tpTimePicker.Value = DateTime.Now;
        }

        /// <summary>
        /// Call method to update the Beginstation ListView.
        /// </summary>
        /// <param name="sender">tbxBeginStation</param>
        private void TbxBeginStation_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLvBeginStation();
        }

        /// <summary>
        /// Catche the DoubleClick event and sets _searchBeginStation to the SelectedItem.
        /// </summary>
        /// <param name="sender">lvBeginStation</param>
        private void LvBeginStation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchBeginStation = (Stop)lbi.Tag;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchEndstation != null)
                btnQuery.IsEnabled = true;
            else
                btnQuery.IsEnabled = false;

            tbxBeginStation.Text = _searchBeginStation.Name;
        }

        /// <summary>
        /// Call method to update the Endstation Listview.
        /// </summary>
        /// <param name="sender">tbxEndStation</param>
        private void TbxEndStation_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLvEndStation();
        }

        /// <summary>
        /// Cath the DoubleClick event and set _searchEndStation to the SelectedItem.
        /// </summary>
        /// <param name="sender">lvEndStation</param>
        private void LvEndStation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchEndstation = (Stop)lbi.Tag;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchBeginStation != null)
                btnQuery.IsEnabled = true;
            else
                btnQuery.IsEnabled = false;

            tbxEndStation.Text = _searchEndstation.Name;
        }

        /// <summary>
        /// Filter based on _searchBeginStation, _searchEndStation, the selected date and time.
        /// Display the results in lvResult.
        /// </summary>
        /// <param name="sender">btnQuery</param>
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            var gefilterdeStations = from station in _stops
                                     where station.Id == _searchBeginStation.Id
                                     select station;

            lvResult.Items.Clear();

            foreach (Stop s in gefilterdeStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = $"[{s.Id}] {s.Name}";
                lbi.Tag = s;
                lvResult.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvResult</param>
        private void LvResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }




        /// <summary>
        /// Call method to update the Trips Listview
        /// </summary>
        /// <param name="sender">tbxStation</param>
        private void TbxStation_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLvTrips();
        }

        /// <summary>
        /// Catch the DoubleClick event and set _searchTripStation.
        /// </summary>
        /// <param name="sender">lvStationTrips</param>
        private void LvStationTrips_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView lv = (ListView)sender;
            try
            {
                ListBoxItem lbi = (ListBoxItem)lv.SelectedItem;
                _searchTripStation = (Stop)lbi.Tag;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (_searchTripStation != null)
                btnQueryTrip.IsEnabled = true;
            else
                btnQueryTrip.IsEnabled = false;

            tbxStation.Text = _searchTripStation.Name;
        }

        /// <summary>
        /// Filter based on _searchTripStation and the selected date.
        /// Display the results in lvTrips.
        /// </summary>
        /// <param name="sender">btnQueryTrip</param>
        private void BtnQueryTrip_Click(object sender, RoutedEventArgs e)
        {
            var gefilterdeTrips = from trip in _stopTimes
                                  where trip.StopId == _searchTripStation.Id.TrimStart('S').Split('_')[0]
                                  orderby trip.TripId.Split(':')[7] ascending
                                  select trip;

            lvTrips.Items.Clear();
            
            foreach (StopTime stopTime in gefilterdeTrips)
            {
                ListBoxItem lbi = new ListBoxItem();
                var stopName = from stop in _stops where stop.Id.TrimStart('S') == stopTime.TripId.Split(':')[4] select stop;
                DateTime dateDT = DateTime.ParseExact(stopTime.TripId.Split(':')[7], "yyyyMMdd", new CultureInfo("fr-FR")).AddHours(Convert.ToDouble(stopTime.DepartureTime.Hours)).AddMinutes(Convert.ToDouble(stopTime.DepartureTime.Minutes));
                lbi.Content = $"[{stopTime.TripId}]\n{dateDT}\n Train to {stopName.First().Name}";
                lbi.Tag = stopTime;
                lvTrips.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Catch the DoubleClick event.
        /// </summary>
        /// <param name="sender">lvTrips</param>
        private void LvTrips_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }



        /// <summary>
        /// Filter the results in lvBeginStation based on the input of tbxBeginStation.
        /// </summary>
        private void UpdateLvBeginStation()
        {
            var filterdStations = from station in _stops
                                  where station.LocationType == LocationType.Station && station.Name.ToLower().Contains(tbxBeginStation.Text.ToLower())
                                  orderby station.Name ascending
                                  select station;

            lvBeginStation.Items.Clear();

            foreach (Stop s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Name;
                lbi.Tag = s;
                lvBeginStation.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Filter the results in lvEndStation based on the input of tbxEndStation.
        /// </summary>
        private void UpdateLvEndStation()
        {
            var filterdStations = from station in _stops
                                  where station.LocationType == LocationType.Station && station.Name.ToLower().Contains(tbxEndStation.Text.ToLower())
                                  orderby station.Name ascending
                                  select station;

            lvEndStation.Items.Clear();

            foreach (Stop s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Name;
                lbi.Tag = s;
                lvEndStation.Items.Add(lbi);
            }
        }

        /// <summary>
        /// Filter the results in lvTrips based on the input of tbxStation
        /// </summary>
        private void UpdateLvTrips()
        {
            var filterdStations = from station in _stops
                                  where station.LocationType == LocationType.Station && station.Name.ToLower().Contains(tbxStation.Text.ToLower())
                                  select station;

            lvStationTrips.Items.Clear();

            foreach (Stop s in filterdStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Name;
                lbi.Tag = s;
                lvStationTrips.Items.Add(lbi);
            }
        }
    }
}
