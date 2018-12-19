using System;
using System.Windows;

namespace Fishezzz
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ResultRealtime : Window
    {
        public ResultRealtime(string titel, string headerInfo, Tuple<string, string, string>[] resultInfo)
        {
            InitializeComponent();
            wdResultRealtime.Title = titel;
            lblInfo.Content = headerInfo;
            lvResult.ItemsSource = resultInfo;
        }
    }
}
