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
        private Bitmap content;
        private int _nindex;

        public Cell(Bitmap source)
        {
            content = source;
            _nindex = 0;
        }

        public Bitmap getSource()
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

        public void setBitmap(Bitmap bmp)
        {
            content = bmp;
        }
    }
}
