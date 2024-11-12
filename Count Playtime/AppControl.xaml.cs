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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Count_Playtime
{
    /// <summary>
    /// Interaction logic for AppControl.xaml
    /// </summary>
    public partial class AppControl : UserControl
    {
        public AppControl(string appName)
        {
            InitializeComponent();
            AppName.Content = appName;
        }
    }
}
