using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kohonen
{
    class DataHandler
    {
        public static void CreateOutput(string folder, Cell[,] map, LabelingHandler lh)
        {
            int mapHeight = map.GetLength(0);
            int mapWidth = map.GetLength(1);

            string mapFolder = Path.Combine(folder, "Map");

            string[,] names = new string[mapHeight, mapWidth];
            int[,] labels = new int[mapHeight, mapWidth];
            
            Directory.CreateDirectory(mapFolder);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    names[i, j] = i + "_" + j;
                    labels[i, j] = map[i, j].getIndex();

                    string dir = Path.Combine(mapFolder, i + "_" + j + ".bmp");
                    map[i, j].SaveAsBmp(dir);
                }
            }

            string[] labelNames = lh.getConvertedLabels().ToArray();
            int[] labelIndexes = lh.getConvertedIndexes().ToArray();

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(folder, "metadata.xml")))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Map");

                    writer.WriteElementString("Height", mapHeight.ToString());
                    writer.WriteElementString("Width", mapWidth.ToString());


                    writer.WriteStartElement("LabelingHandler");
                        writer.WriteElementString("Size", labelNames.Count().ToString());

                        for(int i = 0; i < labelNames.Count(); i++)
                        {
                            writer.WriteStartElement("Label");
                                writer.WriteElementString("Name", labelNames[i]);
                            writer.WriteEndElement();
                        }

                    writer.WriteEndElement();

                    writer.WriteStartElement("CellMap");

                    for (int i = 0; i < mapHeight; i++)
                        {
                        for(int j = 0; j < mapWidth; j++)
                        {
                            writer.WriteStartElement("Cell");
                            writer.WriteElementString("Name", names[i, j]);
                            writer.WriteElementString("Label",labels[i, j].ToString());
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static object[] ProcessInputTrain(string folderImport, string fileLabels)
        {
            if (folderImport == "Not defined")
            {
                System.Windows.MessageBox.Show(" Invalid directory path");
                return null;
            }

            string[] files = Directory.GetFiles(folderImport);

            if (files.Length == 0) { System.Windows.MessageBox.Show(folderImport + " is empty"); return null; }

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
                        return null;

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
                    return null;
                }
            }

            // Label list will be filled manually for testing purposes s

            if (File.Exists(fileLabels))
            {
                string[] extension = fileLabels.Split('.');

                // We will only accept as viable files for the analysis images (of a wide range of extensions)

                if (extension[extension.Length - 1] != "txt" && extension[extension.Length - 1] != "xml")
                {
                    System.Windows.MessageBox.Show(fileLabels + " has not the right file extension");
                    return null;
                }

                if(extension[extension.Length - 1] == "txt")
                {
                    labels = getLabelsFromTxtFile(fileLabels);
                }else
                {
                    labels = getLabelsFromXmlFile(fileLabels);
                }
            }

            return new object[] { labels, sourceData };
        }

        private static List<string> getLabelsFromXmlFile(string fileLabels)
        {
            List<string> labels = new List<string>();

            XmlReader r = new XmlTextReader(fileLabels);

            try
            {
                while (r.Read())
                {
                    switch (r.NodeType)
                    {
                        case XmlNodeType.Attribute:

                            labels.Add(r.Value.ToString());
                            break;

                        default: break;
                    }
                }
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error processing input. Make sure the input has the correct format.");
                return null;
            }

            return labels;
        }

        private static List<string> getLabelsFromTxtFile(string fileLabels)
        {
            List<string> labels = new List<string>();

            string[] lines = System.IO.File.ReadAllLines(fileLabels);  

            for(int i = 0; i < lines.Length; i++)
            {
                labels.Add(lines[i]);
            }

            return labels;
        }

        private static System.Drawing.Image getImageFromFile(string path)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(path);

            return img;
        }

        public static Bitmap ResizeAndConvert(System.Drawing.Image originalImage)
        {
            int size = AdvancedOptions._nBitmapSize;

            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, size, size);
            Bitmap destImage = new Bitmap(originalImage, size, size);

            return destImage;
        }

        public static object[] ProcessInputTest(string inputFolder)
        {
            string[] files = Directory.GetFiles(inputFolder);
            string[] directories = Directory.GetDirectories(inputFolder);

            if (files.Length == 0) { System.Windows.MessageBox.Show(inputFolder + " is empty"); return null; }

            string xmlFile = "";
            string mapFolder = "";

            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i]))
                {
                    string[] extension = files[i].Split('.');

                    // We will only accept as viable files for the analysis images (of a wide range of extensions)

                    if (extension[extension.Length - 1] == "xml")
                    {
                        xmlFile = files[i];
                    }
                }
            }

            for (int i = 0; i < directories.Length; i++)
            {
                if (Directory.Exists(directories[i]))
                {
                    if (directories[i].Split('\\')[directories[i].Split('\\').Length - 1] == "Map")
                    {
                        
                        mapFolder = directories[i];
                    }
                }
            }

            if (xmlFile.Length < 1 || mapFolder.Length < 1)
            {
                System.Windows.MessageBox.Show("Error processing input. Make sure the input has the correct format.");
                return null;
            }

            return getData(xmlFile, mapFolder);
        }

        private static object[] getData(string xmlFile, string mapFolder)
        {
            XmlReader r = new XmlTextReader(xmlFile);

            int w = 0, h = 0;
            Cell[,] map = null;
            List<string> labels = new List<string>();

            try
            {

                while (r.Read())
                {
                    switch (r.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (r.Name)
                            {
                                case "Map":
                                    r.Read();
                                    r.Read();
                                    h = Int32.Parse(r.Value);
                                    r.Read();
                                    r.Read();
                                    r.Read();
                                    w = Int32.Parse(r.Value);
                                    break;

                                case "LabelingHandler":
                                    r.Read();
                                    r.Read();
                                    int size = Int32.Parse(r.Value);
                                    r.Read();

                                    for (int i = 0; i < size; i++)
                                    {
                                        r.Read();
                                        r.Read();
                                        r.Read();
                                        labels.Add(r.Value);
                                        if(labels[labels.Count -1].Length > 1)
                                        {
                                            r.Read();
                                            r.Read();
                                        }
                                    }
                                    break;

                                case "CellMap":

                                    map = new Cell[h, w];

                                    for (int i = 0; i < h; i++)
                                    {
                                        for (int j = 0; j < w; j++)
                                        {
                                            Cell c = ProcessCell(r, mapFolder, i, j);

                                            if (c == null) return null;
                                            map[i, j] = c;
                                        }
                                    }

                                    break;
                            }
                            break;

                        case XmlNodeType.EndElement: break;

                        default: break;
                            
                    }
                }

                if (map.GetLength(0) != h && map.GetLength(1) != w)
                {
                    throw new Exception();
                }


            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error processing input. Make sure the input has the correct format.");
                return null;
            }

            object[] returnable = new object[2];

            returnable[0] = labels;
            returnable[1] = map;

            return returnable;
        }

        private static Cell ProcessCell(XmlReader r, string mapFolder, int x, int y)
        {
            r.Read();
            r.Read();
            r.Read();

            string path = Path.Combine(mapFolder, r.Value + ".bmp");

            Bitmap bmp;
            int label;
            try
            {
                bmp = (Bitmap)Bitmap.FromFile(path);
                r.Read();
                r.Read();
                r.Read();
                label = Int32.Parse(r.Value);
                r.Read();
                r.Read();

            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Error processing input. Make sure the input has the correct format.");
                return null;
            }

            return new Cell(bmp, label, x, y);

        }
    }
}
