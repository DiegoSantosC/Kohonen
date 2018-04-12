using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kohonen
{
    class DataHandler
    {
        public void CreateOutput(string folder, Cell[,] map, LabelingHandler lh)
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

            string[] labelNames = lh.getAllLabels().ToArray();
            int[] labelIndexes = lh.getAllIndexes().ToArray();

            using (XmlWriter writer = XmlWriter.Create(Path.Combine(folder, "metadata.xml")))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Map");
                writer.WriteElementString("Height", mapHeight.ToString());
                writer.WriteElementString("Width", mapWidth.ToString());


                writer.WriteStartElement("LabelingHandler");
                writer.WriteElementString("Size", lh.getAllIndexes().Count.ToString());

                for(int i = 0; i < lh.getAllIndexes().Count; i++)
                {
                    writer.WriteStartElement("Label");
                    writer.WriteElementString("Name", labelNames[i]);
                    writer.WriteElementString("Index", labelIndexes[i].ToString());
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
    }
}
