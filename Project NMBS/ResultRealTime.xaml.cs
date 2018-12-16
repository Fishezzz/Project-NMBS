using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Project_NMBS
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ResultRealTime : Window
    {
        public ResultRealTime(string titel, string headerInfo, Tuple<string, string, string>[] resultInfo)
        {
            InitializeComponent();
            wdResultRealTime.Title = titel;
            lblInfo.Content = headerInfo;
            lvResult.ItemsSource = resultInfo;
        }
    }
}
