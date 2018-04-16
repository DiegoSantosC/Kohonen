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

            Init_TrainingSP();
        }

        private void Mode_Init()
        {
            TrainMode.MouseLeftButtonDown += new MouseButtonEventHandler(SetTrainMode);
            TestMode.MouseLeftButtonDown += new MouseButtonEventHandler(SetTestMode);

        }

        private void Init_TrainingSP()
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Resources\training.jpg", UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            img.Source = src;
            img.Stretch = Stretch.Uniform;

            trainingInProgressSP.Children.Add(img);
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
                TrainFolderSaveLabel.Content = sfd.SelectedPath;
            }
        }


        private void Label_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

            ofd.Title = "Pick a file containing the labels";
            ofd.InitialDirectory = "c:\\";
            ofd.Filter = "txt files (*.txt)|*.txt|xml files (*.xml)|*.xml|All files (*.*)|*.*";
            ofd.FilterIndex = 3;
            ofd.RestoreDirectory = true;

            DialogResult res = ofd.ShowDialog();
            if(res == System.Windows.Forms.DialogResult.OK || !string.IsNullOrWhiteSpace(ofd.FileName.ToString()))
            {
                TrainLabelImportLabel.Content = ofd.FileName.ToString();
            }
        }

        private void SourceTrain_Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();
            
            DialogResult res = sfd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(sfd.SelectedPath))
            {
                TrainFolderImportLabel.Content = sfd.SelectedPath;
            }
        }

        private void SourceTest_Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();

            DialogResult res = sfd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(sfd.SelectedPath))
            {
                TestFolderImportLabel.Content = sfd.SelectedPath;
            }
        }

        private void StartTest_Button_Click(object sender, RoutedEventArgs e)
        {
            Test();

            TestFolderImportLabel.Content = "Not defined";
            ImageElectionLabel.Content = "Not defined";

            TestGrid.Visibility = Visibility.Hidden;
        }

        private void Image_Election_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

            ofd.Title = "Pick the image you wish to test";
            ofd.InitialDirectory = "c:\\";
            ofd.Filter = "jpg files (*.jgp)|*.jpg|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|jpeg files (*.jpeg)|*.jpeg|All files (*.*)|*.*";
            ofd.FilterIndex = 5;
            ofd.RestoreDirectory = true;

            DialogResult res = ofd.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK || !string.IsNullOrWhiteSpace(ofd.FileName.ToString()))
            {
                ImageElectionLabel.Content = ofd.FileName.ToString();
            }
        }

        private void StartTrain_Button_Click(object sender, RoutedEventArgs e)
        {
            Train();

            TrainFolderImportLabel.Content = "Not defined";
            TrainLabelImportLabel.Content = "Not defined";
            TrainFolderSaveLabel.Content = "Not defined";

            TrainGrid.Visibility = Visibility.Hidden;
        }

        private void Train()
        {
            string folderImport = (string)TrainFolderImportLabel.Content;
            string folderLabels = (string)TrainLabelImportLabel.Content;
            string folderSave = (string)TrainFolderSaveLabel.Content;

            if (folderImport == "Not defined" || folderLabels == "Not defined" || folderSave == "Not defined")
            {
                System.Windows.MessageBox.Show("Invalid directory path");
                return;
            }

            object[] returned = DataHandler.ProcessInputTrain(folderImport, folderLabels);

            if(returned == null) { return; }

            List<string> labels = (List<string>)returned[0];
            List<Bitmap> sourceData = (List<Bitmap>)returned[1];

            lh = new LabelingHandler(labels);

            kn = new KohonenNetwork(sourceData, lh, folderSave);

            started = true;

        }

        private void Test()
        {
            string folderImport = (string)TestFolderImportLabel.Content;
            string testImagePath = (string)ImageElectionLabel.Content;

            if (folderImport == "Not defined" || testImagePath == "Not defined")
            {
                System.Windows.MessageBox.Show("Invalid directory path");
                return;
            }

            Bitmap testImage = (Bitmap)Bitmap.FromFile(testImagePath);

            testImage = new Bitmap(testImage, AdvancedOptions._nBitmapSize, AdvancedOptions._nBitmapSize);

            object[] returned = DataHandler.ProcessInputTest(folderImport);

            if (returned == null) { return; }

            List<string> labels = (List<string>)returned[0];
            Cell[,] map = (Cell[,])returned[1];

            lh = new LabelingHandler(labels);
            kn = new KohonenNetwork(testImage, lh, map);

        }
        
        // Creation of a Form from which AdvancedOptions' settings can be modified

        private void Modify_Advanced_Settings_Click(object sender, RoutedEventArgs e)
        {
            Form settingsForm = new Form();
            settingsForm.Width = 900;
            settingsForm.Height = 550;

            System.Windows.Forms.Label title = new System.Windows.Forms.Label();
            title.Location = new System.Drawing.Point(300, 10);
            title.Text = " Advanced Settings ";
            title.Size = new System.Drawing.Size(new System.Drawing.Point(200, 20));
            title.Font = new Font("Arial", 12, System.Drawing.FontStyle.Bold);

            System.Drawing.Size standardSize = new System.Drawing.Size(new System.Drawing.Point(195, 15));

            System.Windows.Forms.Label t1 = new System.Windows.Forms.Label();
            t1.Text = " Kohonen Map settings ";
            t1.Size = new System.Drawing.Size(200, 15);
            t1.Font = new Font("Arial", 9, System.Drawing.FontStyle.Bold);
            t1.Location = new System.Drawing.Point(50, 40);

            System.Windows.Forms.Label l11 = new System.Windows.Forms.Label();
            l11.Text = "  Kohonen Map X size:";
            l11.Size = standardSize;
            l11.Location = new System.Drawing.Point(0, 70);

            System.Windows.Forms.Label l12 = new System.Windows.Forms.Label();
            l12.Text = " Kohonen Map y size: ";
            l12.Size = standardSize;
            l12.Location = new System.Drawing.Point(300, 70);

            System.Windows.Forms.TextBox tb11 = new System.Windows.Forms.TextBox();
            tb11.Size = new System.Drawing.Size(40, 20);
            tb11.Text = AdvancedOptions._nKohonenMapSizeX.ToString();
            tb11.Location = new System.Drawing.Point(210, 70);

            System.Windows.Forms.TextBox tb12 = new System.Windows.Forms.TextBox();
            tb12.Size = new System.Drawing.Size(40, 20);
            tb12.Text = AdvancedOptions._nKohonenMapSizeY.ToString();
            tb12.Location = new System.Drawing.Point(510, 70);

            System.Windows.Forms.Label t3 = new System.Windows.Forms.Label();
            t3.Text = " Learning settings ";
            t3.Size = new System.Drawing.Size(200, 15);
            t3.Font = new Font("Arial", 9, System.Drawing.FontStyle.Bold);
            t3.Location = new System.Drawing.Point(50, 140);


            System.Windows.Forms.Label l31 = new System.Windows.Forms.Label();
            l31.Text = " Initial learning factor:";
            l31.Size = standardSize;
            l31.Location = new System.Drawing.Point(0, 170);

            System.Windows.Forms.Label l32 = new System.Windows.Forms.Label();
            l32.Text = " Final learning factor: ";
            l32.Size = standardSize;
            l32.Location = new System.Drawing.Point(300, 170);

            System.Windows.Forms.TextBox tb31 = new System.Windows.Forms.TextBox();
            tb31.Size = new System.Drawing.Size(40, 20);
            tb31.Text = AdvancedOptions._dInitialLearningFactor.ToString();
            tb31.Location = new System.Drawing.Point(210, 170);

            System.Windows.Forms.TextBox tb32 = new System.Windows.Forms.TextBox();
            tb32.Size = new System.Drawing.Size(40, 20);
            tb32.Text = AdvancedOptions._dFinalLearningFactor.ToString();
            tb32.Location = new System.Drawing.Point(510, 170);

            System.Windows.Forms.Label l41 = new System.Windows.Forms.Label();
            l41.Text = "  Initial radius of action:";
            l41.Size = standardSize;
            l41.Location = new System.Drawing.Point(0, 200);

            System.Windows.Forms.Label l42 = new System.Windows.Forms.Label();
            l42.Text = " Final radius of action: ";
            l42.Size = standardSize;
            l42.Location = new System.Drawing.Point(300, 200);

            System.Windows.Forms.TextBox tb41 = new System.Windows.Forms.TextBox();
            tb41.Size = new System.Drawing.Size(40, 20);
            tb41.Text = AdvancedOptions._nInitialRadius.ToString();
            tb41.Location = new System.Drawing.Point(210, 200);

            System.Windows.Forms.TextBox tb42 = new System.Windows.Forms.TextBox();
            tb42.Size = new System.Drawing.Size(40, 20);
            tb42.Text = AdvancedOptions._nFinalRadius.ToString();
            tb42.Location = new System.Drawing.Point(510, 200);

            System.Windows.Forms.Label l44 = new System.Windows.Forms.Label();
            l44.Text = " Radius modification intensity factor:";
            l44.Size = standardSize;
            l44.Location = new System.Drawing.Point(0, 230);

            System.Windows.Forms.TextBox tb44 = new System.Windows.Forms.TextBox();
            tb44.Size = new System.Drawing.Size(40, 20);
            tb44.Text = AdvancedOptions._dMaxRadiusFactor.ToString();
            tb44.Location = new System.Drawing.Point(210, 230);


            System.Windows.Forms.Label t5 = new System.Windows.Forms.Label();
            t5.Text = " Epoch related settings ";
            t5.Size = new System.Drawing.Size(250, 15);
            t5.Font = new Font("Arial", 9, System.Drawing.FontStyle.Bold);
            t5.Location = new System.Drawing.Point(50, 300);


            System.Windows.Forms.Label l51 = new System.Windows.Forms.Label();
            l51.Text = " Number of epochs:";
            l51.Size = standardSize;
            l51.Location = new System.Drawing.Point(0, 330);

            System.Windows.Forms.Label l52 = new System.Windows.Forms.Label();
            l52.Text = " Epochs until convergence: ";
            l52.Size = standardSize;
            l52.Location = new System.Drawing.Point(300, 330);

            System.Windows.Forms.TextBox tb51 = new System.Windows.Forms.TextBox();
            tb51.Size = new System.Drawing.Size(40, 20);
            tb51.Text = AdvancedOptions._nNumberOfEpochs.ToString();
            tb51.Location = new System.Drawing.Point(210, 330);

            System.Windows.Forms.TextBox tb52 = new System.Windows.Forms.TextBox();
            tb52.Size = new System.Drawing.Size(40, 20);
            tb52.Text = AdvancedOptions._nEpochsUntilConvergence.ToString();
            tb52.Location = new System.Drawing.Point(510, 330);

            System.Windows.Forms.Label l61 = new System.Windows.Forms.Label();
            l61.Text = " Bitmap size:";
            l61.Size = standardSize;
            l61.Location = new System.Drawing.Point(0, 370);

            System.Windows.Forms.TextBox tb61 = new System.Windows.Forms.TextBox();
            tb61.Size = new System.Drawing.Size(40, 20);
            tb61.Text = AdvancedOptions._nBitmapSize.ToString();
            tb61.Location = new System.Drawing.Point(210, 370);


            System.Windows.Forms.Label infoLabel = new System.Windows.Forms.Label();
            infoLabel.Text = "  More info about settings in README file";
            infoLabel.Size = new System.Drawing.Size(400, 15);
            infoLabel.Location = new System.Drawing.Point(50, 480);


            System.Windows.Forms.Button sendBut = new System.Windows.Forms.Button();
            sendBut.Text = "Accept";
            sendBut.Size = new System.Drawing.Size(130, 25);
            sendBut.Location = new System.Drawing.Point(700, 450);
            sendBut.Click += new EventHandler(settingsFormClicked);


            settingsForm.Controls.Add(title);

            settingsForm.Controls.Add(t1);

            settingsForm.Controls.Add(l11);
            settingsForm.Controls.Add(l12);
            settingsForm.Controls.Add(tb11);
            settingsForm.Controls.Add(tb12);

            settingsForm.Controls.Add(t3);

            settingsForm.Controls.Add(l31);
            settingsForm.Controls.Add(l32);
            settingsForm.Controls.Add(tb31);
            settingsForm.Controls.Add(tb32);

            settingsForm.Controls.Add(l41);
            settingsForm.Controls.Add(l42);
            settingsForm.Controls.Add(tb41);
            settingsForm.Controls.Add(tb42);
            settingsForm.Controls.Add(l44);
            settingsForm.Controls.Add(tb44);
      
            settingsForm.Controls.Add(t5);

            settingsForm.Controls.Add(l51);
            settingsForm.Controls.Add(l52);
            settingsForm.Controls.Add(tb51);
            settingsForm.Controls.Add(tb52);

            settingsForm.Controls.Add(l61);
            settingsForm.Controls.Add(tb61);

            settingsForm.Controls.Add(infoLabel);
            settingsForm.Controls.Add(sendBut);


            settingsForm.Show();
        }

        private void settingsFormClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.Button b = (System.Windows.Forms.Button)sender;
            Form form = (Form)b.Parent;

            List<string> parList = new List<string>();

            foreach (object child in form.Controls)
            {
                try
                {
                    System.Windows.Forms.TextBox tb = (System.Windows.Forms.TextBox)child;
                    parList.Add(tb.Text);

                }
                catch (Exception) { }
            }

            ModifyStaticParameters(parList, form);
        }

        private void ModifyStaticParameters(List<string> list, Form f)
        {
            try
            {
                AdvancedOptions._nKohonenMapSizeX = Int32.Parse(list[0]);
                AdvancedOptions._nKohonenMapSizeY = Int32.Parse(list[1]);
                AdvancedOptions._dInitialLearningFactor = Double.Parse(list[2]);
                AdvancedOptions._dFinalLearningFactor = Double.Parse(list[3]);
                AdvancedOptions._nInitialRadius = Int32.Parse(list[4]);
                AdvancedOptions._nFinalRadius = Int32.Parse(list[5]);
                AdvancedOptions._dMaxRadiusFactor = Double.Parse(list[6]);
                AdvancedOptions._nNumberOfEpochs = Int32.Parse(list[7]);
                AdvancedOptions._nEpochsUntilConvergence = Int32.Parse(list[8]);
                AdvancedOptions._nBitmapSize = Int32.Parse(list[9]);

                f.Close();

            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Parameter parsing error");
            }

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
