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
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace Kohonen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Functionality : Graphical support for the Kohonen network to be performed,
    /// input is chosen and results presented
    /// 
    /// Launched by : MainWindow (starting interface)
    /// 
    /// Launches : KohonenAlgorthm
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        private LabelingHandler lh;
        private KohonenNetwork kn;
        private bool started;
        
        public MainWindow()
        {
            InitializeComponent();

            started = false;
            Logo_Init();

            Mode_Init();
        }

        private void Mode_Init()
        {
            TrainMode.MouseLeftButtonDown += new MouseButtonEventHandler(SetTrainMode);
            TestMode.MouseLeftButtonDown += new MouseButtonEventHandler(SetTestMode);

        }

        private void SetTrainMode(object sender, MouseButtonEventArgs e)
        {
            TrainMode.Background = System.Windows.Media.Brushes.DeepSkyBlue;
            TestMode.Background = System.Windows.Media.Brushes.White;

            TrainGrid.Visibility = Visibility.Visible;

            WhipeTestGrid();
        }

        private void SetTestMode(object sender, MouseButtonEventArgs e)
        {
            TrainMode.Background = System.Windows.Media.Brushes.White;
            TestMode.Background = System.Windows.Media.Brushes.DeepSkyBlue;

            TestGrid.Visibility = Visibility.Visible;

            WhipeTrainGrid();
        }

        private void WhipeTestGrid()
        {
            TestGrid.Visibility = Visibility.Hidden;
        }

        private void WhipeTrainGrid()
        {
            TrainGrid.Visibility = Visibility.Hidden;
        }
        private void Logo_Init()
        {
            System.Windows.Controls.Image logo = new System.Windows.Controls.Image();
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Resources\HP_logo.png", UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            logo.Source = src;
            logo.Stretch = Stretch.Uniform;

            LogoSP.Children.Add(logo);
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();

            DialogResult res = sfd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(sfd.SelectedPath))
            {
                FolderSaveLabel.Content = sfd.SelectedPath;
            }
        }

        private void Source_Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();

            DialogResult res = sfd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(sfd.SelectedPath))
            {
                FolderImportLabel.Content = sfd.SelectedPath;

            }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }

       private void Train()
        {
            // not defined!!!
            string folderImport = (string)FolderImportLabel.Content;

            object[] returned = DataHandler.ProcessInputTrain(folderImport);

            if(returned == null) { return; }

            List<string> labels = (List<string>)returned[0];
            List<Bitmap> sourceData = (List<Bitmap>)returned[1];

            string folderSave = (string)FolderSaveLabel.Content;

            lh = new LabelingHandler(labels);

            kn = new KohonenNetwork(sourceData, lh, folderSave);

            started = true;

        }

        private void Test()
        {
            // not defined!!
            string folderImport = (string)FolderImportLabel.Content;
            // string testImagePath = (string)().Content;

            Bitmap testImage = (Bitmap)Bitmap.FromFile(System.IO.Path.Combine("C: ","Users","HP SCDS","Desktop","Dismissed_Captures", "Close_Image.png"));

            testImage = new Bitmap(testImage, AdvancedOptions._nBitmapSize, AdvancedOptions._nBitmapSize);

            object[] returned = DataHandler.ProcessInputTest(folderImport);

            if (returned == null) { return; }

            List<string> labels = (List<string>)returned[0];
            Cell[,] map = (Cell[,])returned[1];
            
            string folderSave = (string)FolderSaveLabel.Content;

            lh = new LabelingHandler(labels);
            kn = new KohonenNetwork(testImage, lh, map, folderSave);

        }
        // UI Closing handling

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (started && kn.isRunning())
            {
                string msg = "Training process running. Close and kill?";

                MessageBoxResult res =
                  System.Windows.MessageBox.Show(
                      msg,
                      "Closing Dialog",
                      MessageBoxButton.YesNo,
                      MessageBoxImage.Warning);

                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }

                else
                {
                    e.Cancel = false;
                    kn.getThread().Abort();
                }
            }
            else
            {
                e.Cancel = false;
            }
        }
    }
}
