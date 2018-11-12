using System;
using System.Collections.Generic;
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
        List<Agency> agencies;
        List<Calendar> calendars;
        List<Calendar_Date> calendar_dates;
        List<Route> routes;
        List<Stop> stops;
        List<Stop_Time> stop_times;
        List<Stop_Time_Override> stop_time_overrides;
        List<Transfer> transfers;
        List<Translation> translations;
        List<Trip> trips;

        public MainWindow()
        {
            InitializeComponent();

            agencies = new List<Agency>();
            calendars = new List<Calendar>();
            calendar_dates = new List<Calendar_Date>();
            routes = new List<Route>();
            stops = new List<Stop>();
            stop_times = new List<Stop_Time>();
            stop_time_overrides = new List<Stop_Time_Override>();
            transfers = new List<Transfer>();
            translations = new List<Translation>();
            trips = new List<Trip>();

            string[] agencyRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\agency.txt");
            for (int i = 1; i < agencyRaw.Length; i++)
                agencies.Add(new Agency(agencyRaw[i]));

            string[] calendarRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\calendar.txt");
            for (int i = 1; i < calendarRaw.Length; i++)
                calendars.Add(new Calendar(calendarRaw[i]));

            string[] calendar_datesRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\calendar_dates.txt");
            for (int i = 1; i < calendar_datesRaw.Length; i++)
                calendar_dates.Add(new Calendar_Date(calendar_datesRaw[i]));

            string[] routesRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\routes.txt");
            for (int i = 1; i < routesRaw.Length; i++)
                routes.Add(new Route(routesRaw[i]));

            string[] stopsRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\stops.txt");
            for (int i = 1; i < stopsRaw.Length; i++)
                stops.Add(new Stop(stopsRaw[i]));

            string[] stop_timesRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\stop_times.txt");
            for (int i = 1; i < stop_timesRaw.Length; i++)
                stop_times.Add(new Stop_Time(stop_timesRaw[i]));
            
            string[] stop_time_overridesRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\stop_time_overrides.txt");
            for (int i = 1; i < stop_time_overridesRaw.Length; i++)
                stop_time_overrides.Add(new Stop_Time_Override(stop_time_overridesRaw[i]));

            string[] transfersRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\transfers.txt");
            for (int i = 1; i < transfersRaw.Length; i++)
                transfers.Add(new Transfer(transfersRaw[i]));
            
            string[] translationsRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\translations.txt");
            for (int i = 1; i < translationsRaw.Length; i++)
                translations.Add(new Translation(translationsRaw[i]));

            string[] tripsRaw = File.ReadAllLines(@"C:\Users\emiel\School\OOP\Project NMBS\GTFS\trips.txt");
            for (int i = 1; i < tripsRaw.Length; i++)
                trips.Add(new Trip(tripsRaw[i]));


            lvAgencies.ItemsSource = agencies;
            lvStops.ItemsSource = stops;
            lvRoutes.ItemsSource = routes;
            lvTrips.ItemsSource = trips;
            lvStopTimes.ItemsSource = stop_times;
            lvCalendar.ItemsSource = calendars;
            lvCalendarDates.ItemsSource = calendar_dates;
            lvTransfers.ItemsSource = transfers;
            lvStopTimeOverrides.ItemsSource = stop_time_overrides;
            lvTranslations.ItemsSource = translations;
        }
    }
}
