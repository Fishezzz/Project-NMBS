using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        GTFSReader<GTFSFeed> reader;
        GTFSFeed feed;

        public MainWindow()
        {
            InitializeComponent();

            reader = new GTFSReader<GTFSFeed>(strict: false);
            feed = reader.Read(new DirectoryInfo("GTFS"));
        }

        private void DpDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            dpDatePicker.DisplayDateStart = feed.Calendars.First().StartDate;
            dpDatePicker.DisplayDateEnd = feed.Calendars.First().EndDate;
        }

        private void TpTimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            tpTimePicker.Value = DateTime.Now;
        }

        private void TbxBeginStation_TextChanged(object sender, TextChangedEventArgs e)
        {
            var gefilterdeStations = from station in feed.Stops
                                     where station.LocationType == LocationType.Station && station.Name.ToLower().Contains(tbxBeginStation.Text.ToLower())
                                     orderby station.Name ascending
                                     select station;

            lvBeginStation.Items.Clear();

            foreach (Stop s in gefilterdeStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Name;
                lbi.Tag = s;
                lvBeginStation.Items.Add(lbi);
            }
        }

        private void TbxEindStation_TextChanged(object sender, TextChangedEventArgs e)
        {
            var gefilterdeStations = from station in feed.Stops
                                     where station.LocationType == LocationType.Station && station.Name.ToLower().Contains(tbxEindStation.Text.ToLower())
                                     orderby station.Name ascending
                                     select station;

            lvEindStation.Items.Clear();

            foreach (Stop s in gefilterdeStations)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = s.Name;
                lbi.Tag = s;
                lvEindStation.Items.Add(lbi);
            }
        }

        private void LvBeginStation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LvEindStation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
