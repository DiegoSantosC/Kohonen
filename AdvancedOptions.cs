using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohonen
{
    /// <summary>
    /// User configurable static parameters to be used during the analysis
    /// </summary>

    class AdvancedOptions
    {
        /*  Use :               KohonenAlgorithm
         *  Definition :        Number of iterations the Kohonen network will execute
         *  Recommended value : Depending on the deepness wanted to obtain from the network
         */

        public static int _nNumberOfEpochs = 100;

        /*  Use :               KohonenAlgorithm
         *  Definition :        Size of the Kohonen map
         *  Recommended value : Depending on the number of different labeled inputs expected to be obtained
         */

        public static int _nKohonenMapSize = 10;

        /*  Use :               KohonenAlgorithm
         *  Definition :        Size of the bitmap the network will work with
         *  Recommended value : Depending on the real size of the inputs 
         */

        public static int _nBitmapSize = 64;

        /*  Use :               KohonenAlgorithm
         *  Definition :        Speed to which the network will adapt to changes
         *  Recommended value : Will depend on the number of epochs, generally chosen so that a full change can be achieved 
         *  after all iterations are executed
         */

        public static double _dLearningFactor = 0.01;
    }
}
