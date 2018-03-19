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

        public MainWindow()
        {
            InitializeComponent();

            //this.Hide();
        }

        private void Source_Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();

            DialogResult res = sfd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(sfd.SelectedPath))
            {
                FolderImportLabel.Content = sfd.SelectedPath;

                ProcessInput();
            }
        }

        private void ProcessInput()
        {

            string folderImport = (string)FolderImportLabel.Content;

            if (folderImport == "Not defined")
            {
                System.Windows.MessageBox.Show(" Invalid directory path");
                return;
            }

            string[] files = Directory.GetFiles(folderImport);

            if (files.Length == 0) { System.Windows.MessageBox.Show(folderImport + " is empty"); return; }

            List<Bitmap> sourceData = new List<Bitmap>();
            List<string> labels = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i]))
                {
                    string[] extension = files[i].Split('.');

                    // We will only accept as viable files for the analysis images (of a wide range of extensions)

                    if (extension[extension.Length - 1] != "png" && extension[extension.Length - 1] != "jpg" && extension[extension.Length - 1] != "jpeg" &&
                        extension[extension.Length - 1] != "bmp")
                    {
                        System.Windows.MessageBox.Show(files[i] + " has not the right file extension");
                        return;

                    }
                    else
                    {
                        System.Drawing.Image img = getImageFromFile(files[i]);
                        sourceData.Add(ResizeAndConvert(img));

                    }
                }
                else
                {

                    System.Windows.MessageBox.Show(files[i] + " is not a valid file");
                    return;
                }
            }

            // Label list will be filled manually for testing purposes s

            labels.Add("Type A");
            labels.Add("Type B");
            labels.Add("Type B");
            labels.Add("Type C");
            labels.Add("Type A");
            labels.Add("Type B");
            labels.Add("Type D");
            labels.Add("Type E");
            labels.Add("Type E");
            labels.Add("Type A");
            labels.Add("Type B");

             lh = new LabelingHandler(labels);

            KohonenNetwork kn = new KohonenNetwork(sourceData, lh);
        }

        private System.Drawing.Image getImageFromFile(string path)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(path);

            return img;
        }

        public Bitmap ResizeAndConvert(System.Drawing.Image originalImage)
        {
            int size = AdvancedOptions._nBitmapSize;

            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, size, size);
            Bitmap destImage = new Bitmap(size, size);

            destImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(destImage, destRect, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            string folder = System.IO.Path.Combine("C:", "Users", "HP SCDS", "Desktop");

            //destImage.Save(folder + "Test.bmp");

            return destImage;
        }
    }
}
