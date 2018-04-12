using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kohonen
{
    /// <summary>
    /// 
    /// User defined type cell, which contains a randomly generated,  unlabeled bitmap as part of the Kohonen map that will be
    /// modified and potentially labeled in successive iterations of the network algorithm
    /// 
    /// </summary>

    class Cell
    {
        private float[][][] content;
        private int _nindex;
        private int posX, posY;

        public Cell(float[][][] source, int x, int y)
        {
            content = source;
            _nindex = -1;
            posX = x;
            posY = y;
        }

        public float[][][] getSource()
        {
            return content;
        }

        public int getIndex()
        {
            return _nindex;
        }

        public void setIndex(int index)
        {
            _nindex = index;
        }

        public void setBitmap(float[][][] vector)
        {
            content = vector;
        }

        public int getX()
        {
            return posX;
        }
        public int getY()
        {
            return posY;
        }

        public void SaveAsBmp(string folder)
        {
            int h = content.Length;
            int w = content[0].Length;
            Bitmap bmp = new Bitmap(h, w);

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++) bmp.SetPixel(i, j, Color.FromArgb((int)content[i][j][0], (int)content[i][j][1], (int)content[i][j][2]));

            bmp.Save(folder);
        }
    }
}
