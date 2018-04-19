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

        public static int _nNumberOfEpochs = 10;

        /*  Use :               KohonenAlgorithm
         *  Definition :        Size of the Kohonen map (X, Y)
         *  Recommended value : Depending on the number of different labeled inputs expected to be obtained
         */

        public static int _nKohonenMapSizeX = 10, _nKohonenMapSizeY = 10;

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

        public static double _dInitialLearningFactor = 0.1, _dFinalLearningFactor = 0.001;

        /*  Use :               KohonenAlgorithm
        *  Definition :         Initial and final radius (in cells) within which the cells will modified after an epoch to match the winner
        *  Recommended value :  Depending on the deepness the network is designed to have, as well as the expected number of labels, 
        *  in this case will be initialized as half of the map's size
        */
        //(Comment toroidal /2)
        public static int _nInitialRadius = (int)Math.Sqrt((Math.Pow(_nKohonenMapSizeX/2, 2) + Math.Pow(_nKohonenMapSizeY/2, 2))),  _nFinalRadius = 1;

        /*  Use :               KohonenAlgorithm
         *  Definition :        Number of epochs until the influence radius, as well as the learning factor, converge
         *  Recommended value : Will depend on the number of epochs, generally 70% / 80%
         */

        public static int _nEpochsUntilConvergence = 80;

        /*  Use :              KohonenAlgorithm
        *   Definition :        Factor that defines the rate in which the intensity of the modification after the best match in an epoch is found will diminish
        *                       when getting far from that best cell
        *   Recommended value : 0.05, will depend on the global impact to the map we want to achieve in an iteration
        */
        public static double _dMaxRadiusFactor = 0.05;
           
    
    }
}
