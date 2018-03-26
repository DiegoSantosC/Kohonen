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
                     
        public KohonenNetwork(List<Bitmap> data, LabelingHandler labels)
        {
            sourceData = data;
            lh = labels;

            for (int i = 0; i < lh.getAllIndexes().Count; i++) Console.Write(lh.getAllIndexes()[i] + " ");

            Console.WriteLine();

            Init_Map();

            Train_Network();

        }

        private void Init_Map()
        {
            int size = AdvancedOptions._nKohonenMapSize;

            map = new Cell[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++) map[i, j] = generateCell();
        }

        private double ComputeDistanceFactor(int currEpoch, double distance)
        {
            // The distance factor will be computed so that the change applied to far away cells from the winner one is negligible,
            // and is exctracted from the formula "e^(c*dist(AB))" , being c the distance factor, and dist(AB) the Euclidean distance
            // between cell A (the winner) and B (the current one)

            if (currEpoch > AdvancedOptions._nEpochsUntilConvergence)
                currEpoch = AdvancedOptions._nEpochsUntilConvergence;

            double posEpoch = (currEpoch - 0) / (AdvancedOptions._nEpochsUntilConvergence - 0);
            double currRadius = AdvancedOptions._nFinalRadius + posEpoch * (AdvancedOptions._nInitialRadius - AdvancedOptions._nFinalRadius);
            double F = Math.Log(0.01) / (Math.Pow(currRadius, 1) + 1E-11);

            return F;
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

               int[] bestMatch = FindBestMatch(sourceData[i], i);

                // The cell is labelled and it's neightbours modified

                if (bestMatch[0] == -1)
                {
                    MessageBox.Show("No available cell found");

                    return;
                }

                ModifyMap(bestMatch, sourceData[i], epoch);
            }
        }

        private int[] FindBestMatch(Bitmap bitmap, int index)
        {
            int[] bestMatch = new int[2] { -1, -1};
            double bestDifference = double.MaxValue;

            for(int i = 0; i < map.GetLength(0); i++)
            {
                for(int j = 0; j < map.GetLength(1); j++)
                {
                    double difference = ComputeDifference(bitmap, map[i, j].getSource());

                    // Take best match considering label

                    if (i == 0 && j == 0)
                    {
                        if(map[i,j].getIndex() == 0 || map[i,j].getIndex() == lh.getIndex(i))
                        {
                            bestMatch = new int[] { 0, 0 };
                            bestDifference = difference;
                        }
                    }
                    else
                    {
                        if(difference < bestDifference)
                        {
                            if (map[i, j].getIndex() == 0 || map[i, j].getIndex() == lh.getIndex(i))
                            {

                                bestMatch = new int[] { i, j };
                                bestDifference = difference;
                            }
                        }
                    }
                }
            }

            return bestMatch;
        }

        private void ModifyMap(int[] pos,  Bitmap sourceBmp, int currEpoch)
        {
            for(int i = 0; i < AdvancedOptions._nBitmapSize; i++)
            {
                for (int j = 0; j < AdvancedOptions._nBitmapSize; j++)
                {
                    double distance = Math.Pow(pos[0] - i, 2) + Math.Pow(pos[1] - j, 2);

                    ModifyCell(map[i,j], distance,  sourceBmp, currEpoch);
                }
            }
        }

        private void ModifyCell(Cell cell, double distance, Bitmap sourceBmp, int currEpoch)
        {
            Bitmap bmp = cell.getSource();

            for(int i = 0; i < sourceBmp.Height; i++)
            {
                for(int j = 0; j < sourceBmp.Width; j++)
                {
                    bmp.SetPixel(i, j, ModifiedPixel(bmp.GetPixel(i, j), sourceBmp.GetPixel(i, j), distance, currEpoch));
                }
            }
        }

        private Color ModifiedPixel(Color color1, Color color2, double distance, int currEpoch)
        {
            double k = ComputeLearningFactor(currEpoch);

            int diffR = color2.R - color1.R;
            int diffG = color2.G - color1.G;
            int diffB = color2.B - color1.B;

            double distanceFactor = ComputeDistanceFactor(currEpoch, distance);

            if(distanceFactor == -1)
            {
                return color1;
            }
            else
            {
                int newR = (int)(color1.R + k * diffR * Math.Exp(distanceFactor * distance));
                int newG = (int)(color1.G + k * diffG * Math.Exp(distanceFactor * distance));
                int newB = (int)(color1.B + k * diffB * Math.Exp(distanceFactor * distance));

                return Color.FromArgb(newR, newG, newB);
            }
        }

        private double ComputeLearningFactor(int epoch)
        {
            if (epoch > AdvancedOptions._nEpochsUntilConvergence) epoch = AdvancedOptions._nEpochsUntilConvergence;

            return (AdvancedOptions._dFinalLearningFactor - AdvancedOptions._dInitialLearningFactor) * (epoch/AdvancedOptions._nEpochsUntilConvergence) + AdvancedOptions._dInitialLearningFactor;           

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
