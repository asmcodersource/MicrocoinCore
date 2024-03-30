using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Mining
{
    public static class Complexity
    {
        static private double[] calculatedAvgIterationsToMine = new double[256];

        static Complexity()
        {
            for( int i = 0; i < 256; i++ )
                calculatedAvgIterationsToMine[i] = -1.0/ Math.Log((Math.Pow(2, i) - 1) / Math.Pow(2, i)) - 1.0; // 99.99% probality of overflow for big value of complexty
        }

        static public double GetAverageIterationsToMine(int targetComplexity)
        {
            if (targetComplexity >= 256)
                throw new ArgumentException("Complexity can't be more than 256 for SHA256 hashing");
            return calculatedAvgIterationsToMine[targetComplexity];
        }

        static public int GetClosestComplexity(double avgIterationsOfMine)
        {
            for( int i = 1; i < 256; i++)
            {
                var firstAvgIterations = calculatedAvgIterationsToMine[i - 1];
                var secondAvgIterations = calculatedAvgIterationsToMine[i + 0];
                if (firstAvgIterations < avgIterationsOfMine && avgIterationsOfMine < secondAvgIterations)
                {
                    var average = firstAvgIterations / 2.0 + secondAvgIterations / 2.0;
                    return avgIterationsOfMine > average ? i : i - 1;
                }
            }
            return 255;
        }
    }
}
