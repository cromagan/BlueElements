using static BlueBasics.Constants;

namespace BlueBrain
{
    public class Neurone
    {

        public double[] Weight { get; internal set; }


        public double Error { get; internal set; }

        public double Value { get; internal set; }

        public string Name { get; internal set; }

        public Neurone(int NeuroneCountOnNextLayer)
        {
            Weight = new double[NeuroneCountOnNextLayer];
            for (var i = 0 ; i < Weight.Length ; i++)
            {
                Weight[i] = GlobalRND.NextDouble() * 2 - 1;
            }


        }
   }
}
