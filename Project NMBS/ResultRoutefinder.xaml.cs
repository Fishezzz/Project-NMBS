using System;
using System.Windows;

namespace Fishezzz
{
    /// <summary>
    /// Interaction logic for ResultRoutefinder.xaml
    /// </summary>
    public partial class ResultRoutefinder : Window
    {
        public ResultRoutefinder(string titel, string headerInfo, Tuple<string, string, string>[] resultInfo)
        {
            InitializeComponent();
            wdResultRoutefinder.Title = titel;
            lblInfo.Content = headerInfo;
            lvResult.ItemsSource = resultInfo;
        }
    }
}
