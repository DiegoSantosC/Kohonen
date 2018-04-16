using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Kohonen
{
    class KohonenNetwork
    {
        private List<Bitmap> sourceData;
        private LabelingHandler lh;
        private Cell[,] map;
        private Cell best;
        private Thread thread;
        private bool running;
        private string outputFolder;
             
        // Version of the KohonenNetwork for it to be trained
                
        public KohonenNetwork(List<Bitmap> data, LabelingHandler labels, string save)
        {
            sourceData = data;
            lh = labels;

            Init_Map();

            thread = new Thread(Train_Network);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            running = true;

            outputFolder = save;
        }

        // Trained version of the KohonenNetwork for it to be tested against a new input.
        // As such, the trained network must be added as a parameter

        public KohonenNetwork(Bitmap data, LabelingHandler labels, Cell[,] srcMap, string save)
        {
            sourceData = new List<Bitmap> { data };
            lh = labels;

            map = srcMap;
            outputFolder = save;

            Test_Network();
        }

        private void Init_Map()
        {
            int size = AdvancedOptions._nKohonenMapSize;

            map = new Cell[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++) map[i, j] = generateCell(i, j);
        }

        private double ComputeDistanceFactor(int currEpoch, double distance)
        {
            // The distance factor will be computed so that the change applied to far away cells from the winner one is negligible,
            // and is exctracted from the formula "e^(c*dist(AB))" , being c the distance factor, and dist(AB) the Euclidean distance
            // between cell A (the winner) and B (the current one)

            if (currEpoch > AdvancedOptions._nEpochsUntilConvergence)
                currEpoch = AdvancedOptions._nEpochsUntilConvergence;

            double posEpoch = (AdvancedOptions._nEpochsUntilConvergence - currEpoch) / (AdvancedOptions._nEpochsUntilConvergence - 0);
            double currRadius = AdvancedOptions._nFinalRadius + posEpoch * (AdvancedOptions._nInitialRadius - AdvancedOptions._nFinalRadius);
            double F = Math.Log(AdvancedOptions._dMaxRadiusFactor) / (Math.Pow(currRadius, 2) + 1E-11);

            return F;
        }

        private void Test_Network()
        {
            int[] bestMatch = FindBestMatch(sourceData[0],  -1);

            best = map[bestMatch[0], bestMatch[1]];

            Console.WriteLine(best.getX() + " " + best.getY());
            Console.WriteLine(best.getIndex());

        }

        private void Train_Network()
        {
            int nIterations = AdvancedOptions._nNumberOfEpochs;

            for (int i = 0; i < nIterations; i++) Execute_Epoch(i);

            DataHandler.CreateOutput(outputFolder, map, lh);

            App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                     new Action(() => finishedTraining()));
        }
        
        private void Execute_Epoch(int epoch)
        {
            for(int i = 0; i < sourceData.Count; i++)
            {
                // Best match is found attending to Euclidean distance

                int label = lh.getIndex(i);

                int[] bestMatch = FindBestMatch(sourceData[i], label);
                
                // The cell is labelled and it's neightbours modified

                if (bestMatch[0] == -1)
                {
                    MessageBox.Show("No available cell found");

                    return;
                }

                map[bestMatch[0], bestMatch[1]].setIndex(label);
                

                ModifyMap(bestMatch, sourceData[i], epoch);

                Console.WriteLine(epoch + " " + i);
            }
        }

        private int[] FindBestMatch(Bitmap bitmap, int label)
        {
            int[] bestMatch = new int[2] { -1, -1};
            double bestDifference = double.MaxValue;

            for(int i = 0; i < map.GetLength(0); i++)
            {
                for(int j = 0; j < map.GetLength(1); j++)
                {
                    // Take best match considering label

                    if ( label == -1 || map[i, j].getIndex() == -1 || map[i, j].getIndex() == label)
                    {
                        // If the cell's label is adequate, compute difference

                        double difference = ComputeDifference(bitmap, map[i, j].getSource());

                        // If the difference is the least, take as winner

                        if (difference < bestDifference)
                        {
                            bestMatch = new int[] { i, j };
                            bestDifference = difference;
                        }
                    }
                }
            }

            //Console.WriteLine(bestDifference);

            return bestMatch;
        }

        private void ModifyMap(int[] pos,  Bitmap sourceBmp, int currEpoch)
        {
            for(int i = 0; i < AdvancedOptions._nKohonenMapSize; i++)
            {
                for (int j = 0; j < AdvancedOptions._nKohonenMapSize; j++)
                {
                    double distance = Math.Pow(pos[0] - i, 2) + Math.Pow(pos[1] - j, 2);

                    ModifyCell(map[i,j], distance,  sourceBmp, currEpoch);
                }
            }
        }

        private void ModifyCell(Cell cell, double distance, Bitmap sourceBmp, int currEpoch)
        {
            float[][][] bmp = cell.getSource();

            for(int i = 0; i < sourceBmp.Height; i++)
            {
                for(int j = 0; j < sourceBmp.Width; j++)
                {
                    bmp[i][j] = ModifiedPixel(bmp[i][j], sourceBmp.GetPixel(i, j), distance, currEpoch);
                }
            }
        }

        private float[] ModifiedPixel(float[] color1, Color color2, double distance, int currEpoch)
        {
            double k = ComputeLearningFactor(currEpoch);

            float diffR = color2.R - color1[0];
            float diffG = color2.G - color1[1];
            float diffB = color2.B - color1[2];

            double distanceFactor = ComputeDistanceFactor(currEpoch, distance);

            if(distanceFactor == -1)
            {
                return color1;
            }
            else
            {
                float newR = (float)(color1[0] + k * diffR * Math.Exp(distanceFactor * distance));
                float newG = (float)(color1[1] + k * diffG * Math.Exp(distanceFactor * distance));
                float newB = (float)(color1[2] + k * diffB * Math.Exp(distanceFactor * distance));

                return new float[]{ newR, newG, newB };
            }
        }

        private double ComputeLearningFactor(int epoch)
        {
            if (epoch > AdvancedOptions._nEpochsUntilConvergence) epoch = AdvancedOptions._nEpochsUntilConvergence;

            return (AdvancedOptions._dFinalLearningFactor - AdvancedOptions._dInitialLearningFactor) * (epoch/AdvancedOptions._nEpochsUntilConvergence) + AdvancedOptions._dInitialLearningFactor;           

        }

        private double ComputeDifference(Bitmap bitmap1, float[][][] bitmap2)
        {
            double acc = 0;

            for (int i = 0; i < bitmap1.Height; i++)
                for(int j = 0; j < bitmap1.Width; j++)
                    acc += PixelDifference(bitmap1.GetPixel(i,j), bitmap2[i][j]);
                
            return acc;
        }

        private double PixelDifference(Color c1, float[] c2)
        {
            return (Math.Sqrt(Math.Pow((c1.R - c2[0]), 2) + Math.Pow((c1.G - c2[1]), 2) + Math.Pow((c1.B - c2[2]), 2))); 
        }

        private Cell generateCell(int x, int y)
        {
            int size = AdvancedOptions._nBitmapSize;

            float[][][] bmp = new float[size][][];

            Random r = new Random();

            for (int i = 0; i < size; i++)
            {
                bmp[i] = new float[size][];
                for (int j = 0; j < size; j++) bmp[i][j] = generateRandomPixel(r);

            }

            return new Cell(bmp, x, y);
        }

        private Cell generateCell(Bitmap bmp, int x, int y)
        {
            int h = bmp.Height;
            int w = bmp.Width;

            float[][][] source = new float[h][][];

            for(int i = 0; i < h; i++)
            {
                source[i] = new float[w][];
                for(int j = 0; j < w; j++)
                {
                    source[i][j] = new float[] { bmp.GetPixel(i, j).R, bmp.GetPixel(i, j).G, bmp.GetPixel(i, j).B };
                }
            }

            return new Cell(source, x, y);
        }

        private float[] generateRandomPixel(Random r)
        {
            float R = r.Next(0, 255);
            float G = r.Next(0, 255);
            float B = r.Next(0, 255);

            return new float[] { R, G, B };
        }

        private void finishedTraining()
        {
            running = false;
        }

        public bool isRunning()
        {
            return running;
        }

        public Thread getThread()
        {
            return thread;
        }
    }
}
