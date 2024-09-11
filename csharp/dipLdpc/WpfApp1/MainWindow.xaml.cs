using dipLdpc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigObj.loadOptsFromFile();
            wnd = this;
            dsp = wnd.Dispatcher;
            ConfigObj cfgObj = ConfigObj.CurrentConfig();
            propGrid.SelectedObject = cfgObj;
            txtBoxLst.Add("Load");
            textBox.ItemsSource = txtBoxLst;

        }
        static MainWindow wnd;
        static internal void printOutput(string str)
        {
            //wnd.rtextBox.AppendText($"{str}\n");
            CT_logMessageByFrm(str);
        }
        private void hbbRsErrBtn_Click(object sender, RoutedEventArgs e)
        {
            //rtextBox.AppendText("Start\n");

            graphBtn_Click();
        }
        private void graphBtn_Click()
        {
            //D:\tmpG8\ken\OTA\DC0924153556-LBb.bin
            double[] arr = { 120, 100, 98, 200, 100, 170 };
            double[] arr2 = { 100, 98, 200, 100, 170 , 120 };
            double[] Xarr = { 1, 2, 3, 4 ,5,26};
            /*int[] arr1 = new int[200];
            for (int i = 0; i < arr1.Length; i++)
            {
                arr1[i] = arr[i % arr.Length];
            }*/
            var gpb = CustomControls.graphXYControl.getDblXYGraph(arr, Xarr, "Array 1");
            gpb.AddYarray(arr2, "Array 2");
            graphView graph = new graphView(gpb);
            //graph.setXlimits(1, 3);
            graph.Owner = this;
            graph.Show();
        }

        public static void makeGraph(double[] Yarr, double[] Xarr, double[] Y2arr)
        {
            Yarr = Array.ConvertAll(Yarr, y => Math.Log10(y));
            var gpb = CustomControls.graphXYControl.getDblXYGraph(Yarr, Xarr, "arr1");
            Y2arr = Array.ConvertAll(Y2arr, y => y != 0 ? Math.Log10(y):double.NegativeInfinity);
            gpb.AddYarray(Y2arr, "Y2arr");
            graphView graph = new graphView(gpb);
            //graph.setXlimits(1, 3);
            graph.Owner = wnd;
            graph.Show();
        }
        private void BP_Start_Click(object sender, RoutedEventArgs e)
        {
            CodecMain simul = new CodecMain();
            simul.start();
        }

        private void CodeWordTest_Click(object sender, RoutedEventArgs e)
        {
            CodecMain simul = new CodecMain();
            simul.GenCodeWordTest();

        }

        private void GaussTst_Click(object sender, RoutedEventArgs e)
        {
            Channel chn = new Channel(null, 1, 1);
            int[] arr = chn.test_generateAWGN();//
            double xTrK = Channel.getXtransK();
            int xZero = Channel.getXzero();
            int[] arr2 = Channel.Arr2Func();
            int[] arr3 = chn.Arr3Func();
            var gpb = CustomControls.graphXYControl.getIntGraph(arr, xTrK, xZero, "ExperimentWithJavaCode");
            gpb.AddYarray(arr2, "Theory");
            gpb.AddYarray(arr3, "ExperimentWithSumOfRndms");
            graphView graph = new graphView(gpb);

            graph.Owner = this;
            graph.Show();
        }

        private void WavTst_Click(object sender, RoutedEventArgs e)
        {
            RunMain.WavReaderMain();
        }

        static Dispatcher dsp;
        static int prevTicks = Environment.TickCount;
        static ObservableCollection<string> txtBoxLst = new ObservableCollection<string>();
        public static void CT_logMessageByFrm(string msg)
        {
            int ticks = Environment.TickCount;
            int tdiff = ticks - prevTicks;
            if (tdiff > 100)
            {
                prevTicks = ticks;
            }
            dsp.Invoke(new Action(() =>
            {
                //                ListBoxItem itm = new ListBoxItem();
                //               itm.Content = msg;
                /*int indx = wnd.textBox.Items.Add(msg);
                if (tdiff > 100)
                {
                    wnd.textBox.ScrollIntoView(wnd.textBox.Items[indx]);
                }*/
                txtBoxLst.Add(msg);
                if(wnd.textBox.SelectedIndex == -1)
                if (VisualTreeHelper.GetChildrenCount(wnd.textBox) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(wnd.textBox, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }
            }));
            //            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            //            wnd.textBox.AppendText(msg);
        }

        private void ConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            //            ConfigObj cfgObj = ConfigObj.CurrentConfig();
            //            ConfigFrm frm = new ConfigFrm(cfgObj);
            //          frm.Show();*/
            //            propGrid.SelectedObject = cfgObj;
            ConfigObj.saveAllToFile();
        }
    }
}
