using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dipLdpc
{
    /// <summary>
    /// Interaction logic for ConfigFrm.xaml
    /// </summary>
    public partial class ConfigFrm : Window
    {
        public ConfigFrm(ConfigObj cfg)
        {
            InitializeComponent();
            propGrid2.SelectedObject = cfg;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            ConfigObj.saveAllToFile();
            Close();
        }
    }
}
