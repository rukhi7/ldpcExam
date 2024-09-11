using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for graphView.xaml
    /// </summary>
    public partial class graphView : Window
    {
        RadioButton[] clstSwitch = new RadioButton[5];
        CustomControls.GraphProcBase gProc;

        internal graphView(CustomControls.GraphProcBase gpb)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            gProc = gpb;
            libGraph.setArray(gpb);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        
    }
}
