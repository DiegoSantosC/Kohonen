using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kohonen
{
    class KohonenNetwork
    {
        private List<Bitmap> sourceData;
        private LabelingHandler lh;
        private Cell[,] map;
        private double distanceFactor;
                     
        public KohonenNetwork(List<Bitmap> data, LabelingHandler labels)
        {
            sourceData = data;
            lh = labels;

            for (int i = 0; i < lh.getAllIndexes().Count; i++) Console.Write(lh.getAllIndexes()[i] + " ");

            Console.WriteLine();

            Init_Map();

            ComputeDistanceFactor();

            Train_Network();

        }

        private void Init_Map()
        {
            int size = AdvancedOptions._nKohonenMapSize;

            map = new Cell[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++) map[i, j] = generateCell();
        }

        private void ComputeDistanceFactor()
        {
            // The distance factor will be computed so that the change applied to far away cells from the winner one is negligible,
            // and is exctracted from the formula "e^(c*dist(AB))" , being c the distance factor, and dist(AB) the Euclidean distance
            // between cell A (the winner) and B (the current one)

            distanceFactor = Math.Log(0.01) / (AdvancedOptions._nBitmapSize / 2);
        }

        private void Train_Network()
        {
            int nIterations = AdvancedOptions._nNumberOfEpochs;

            for (int i = 0; i < nIterations; i++) Execute_Epoch(i);
        }

        private void Execute_Epoch(int epoch)
        {
            for(int i = 0; i < sourceData.Count; i++)
            {
                // Best match is found attending to Euclidean distance

                List<int[]> bestMatches = FindBestMatches(sourceData[i]);

                // The cell is labelled and it's neightbours modified

                bool allocated = false;
                int[] chosenMatch = new int[2];

                for (int n = 0; n < bestMatches.Count; n++)
                {
                    if(map[bestMatches[n][0], bestMatches[n][1]].getIndex() == 0 || map[bestMatches[n][0], bestMatches[n][1]].getIndex() == lh.getIndex(i))
                    {
                        allocated = true;
                        map[bestMatches[n][0], bestMatches[n][1]].setIndex(lh.getIndex(i));
                        chosenMatch = bestMatches[n];
                        break;
                    }
                }
                if (!allocated)
                {
                    MessageBox.Show("No available cell found");

                    return;
                }

                ModifyMap(chosenMatch, sourceData[i]);
            }
        }

        private List<int[]> FindBestMatches(Bitmap bitmap)
        {
            List<int[]> bestMatches = new List<int[]>();
            List<double> bestDifferences = new List<double>();

            for(int i = 0; i < map.GetLength(0); i++)
            {
                for(int j = 0; j < map.GetLength(1); j++)
                {
                    double difference = ComputeDifference(bitmap, map[i, j].getSource());

                    if (i == 0 && j == 0)
                    {
                        bestMatches.Add(new int[] { 0, 0 });
                        bestDifferences.Add(difference);
                    }
                    else
                    {
                        bool allocated = false;

                        for (int n = 0; n < bestDifferences.Count; n++)
                        {
                            if (difference < bestDifferences[n])
                            {
                                bestDifferences.Insert(n, difference);
                                bestMatches.Insert(n, new int[] { i, j });
                                allocated = true;

                                break;
                            }
                        }

                        if (!allocated)
                        {
                            bestDifferences.Add(difference);
                            bestMatches.Add(new int[] { i, j });
                        }
                    }                  
                }
            }

            for(int i = 0; i < bestDifferences.Count; i++)
            {
                Console.WriteLine(bestDifferences[i] + "   " + bestMatches[i][0] + " " + bestMatches[i][1]);
            }

            return bestMatches;
        }

        private void ModifyMap(int[] pos,  Bitmap sourceBmp)
        {
            for(int i = 0; i < AdvancedOptions._nBitmapSize; i++)
            {
                for (int j = 0; j < AdvancedOptions._nBitmapSize; j++)
                {
                    double distance = Math.Sqrt(Math.Pow(pos[0] - i, 2) + Math.Pow(pos[1] - j, 2));

                    ModifyCell(map[i,j], distance,  sourceBmp);
                }
            }
        }

        private void ModifyCell(Cell cell, double distance, Bitmap sourceBmp)
        {
            Bitmap bmp = cell.getSource();

            for(int i = 0; i < sourceBmp.Height; i++)
            {
                for(int j = 0; j < sourceBmp.Width; j++)
                {
                    bmp.SetPixel(i, j, ModifiedPixel(bmp.GetPixel(i, j), sourceBmp.GetPixel(i, j), distance));
                }
            }
        }

        private Color ModifiedPixel(Color color1, Color color2, double distance)
        {
            double k = AdvancedOptions._dLearningFactor;

            int diffR = color2.R - color1.R;
            int diffG = color2.G - color1.G;
            int diffB = color2.B - color1.B;

            int newR = (int) (color1.R + k * diffR * (Math.Pow(Math.E , distanceFactor * distance)));
            int newG = (int)(color1.G + k * diffG * (Math.Pow(Math.E , distanceFactor * distance)));
            int newB = (int)(color1.B + k * diffB * (Math.Pow(Math.E , distanceFactor * distance)));

            return Color.FromArgb(newR, newG, newB);
        }

        private double ComputeDifference(Bitmap bitmap1, Bitmap bitmap2)
        {
            double acc = 0;

            for (int i = 0; i < bitmap1.Height; i++)
                for(int j = 0; j < bitmap1.Width; j++)
                    acc += PixelDifference(bitmap1.GetPixel(i,j), bitmap2.GetPixel(i,j));
                
            return acc;
        }

        private double PixelDifference(Color c1, Color c2)
        {
            return (Math.Sqrt(Math.Pow((c1.R - c2.R), 2) + Math.Pow((c1.G - c2.G), 2) + Math.Pow((c1.B - c2.B), 2))); 
        }

        private Cell generateCell()
        {
            int size = AdvancedOptions._nBitmapSize;

            Bitmap bmp = new Bitmap(size, size);

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++) bmp.SetPixel(i, j, generateRandomPixel());

            return new Cell(bmp);
        }

        private Color generateRandomPixel()
        {
            Random r = new Random();
            int R = r.Next(0, 255);
            int G = r.Next(0, 255);
            int B = r.Next(0, 255);


            return Color.FromArgb(R, G, B);
        }
    }
}
