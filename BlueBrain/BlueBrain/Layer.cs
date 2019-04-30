using System;
using System.Collections.Generic;
using System.Linq;
using static BlueBasics.Develop;
using static BlueBasics.Constants;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueBrain
{

    public class Layer
    {
 
        public Neurone[] Neurones { get; internal set; }

        public double[] Bias { get; internal set; }
        public double[] BiasWeight { get; internal set; }


        public Layer(int NeuroneCount, int NeuronCountOnNextLayer)
        {
            Neurones = new Neurone[NeuroneCount];
            for (var i = 0 ; i < Neurones.Length ; i++)
            {
                Neurones[i] = new Neurone(NeuronCountOnNextLayer);
            }

            Bias = new double[NeuronCountOnNextLayer];
            BiasWeight = new double[NeuronCountOnNextLayer];

            for (var i = 0 ; i < BiasWeight.Length ; i++)
            {
                BiasWeight[i] = GlobalRND.NextDouble() * 2 - 1;
                Bias[i] = GlobalRND.NextDouble() < 0.5 ? 1 : -1;

            }
        }

        public void Compute(Layer ParentLayer)
        {
            if (ParentLayer == null) { return; }
            for (var j = 0 ; j < Neurones.Length ; j++)
            {
                var x = 0.0;
                for (var i = 0 ; i < ParentLayer.Neurones.Length ; i++)
                {
                    x += ParentLayer.Neurones[i].Value * ParentLayer.Neurones[i].Weight[j];
                }
                x += ParentLayer.Bias[j] * ParentLayer.BiasWeight[j];

                var k = Math.Exp(x);
                Neurones[j].Value = k / (1.0 + k);

                if (double.IsInfinity(Neurones[j].Value)) { DebugPrint(enFehlerArt.Fehler, "Undendlichkeit entdeckt"); }

            }
        }

        internal void SetNames(List<string> Names)
        {
            for (var z = 0 ; z < Names.Count ; z++)
            {
                Neurones[z].Name = Names[z];
            }
        }

        public void SetValue(string Name, double Value)
        {
            foreach (var t in Neurones)
            {
                if (t.Name == Name)
                {
                    t.Value = Value;
                    return;
                }
            }

            DebugPrint(enFehlerArt.Fehler, "Value Name nicht gefunden: '" + Name + "'");
        }




        public string SoftMax()
        {
            var Highest = double.MinValue;
            var N = string.Empty;

            foreach (var t in Neurones)
            {
                if (t.Value > Highest)
                {
                    Highest = t.Value;
                    N = t.Name;
                }
            }

            return N;
        }

        public double SoftMaxValue(string OfName)
        {
            var FoundValue = double.MinValue;
            double AllValues = 0;

            foreach (var t in Neurones)
            {
                if (t.Value > 0)
                {
                    if (t.Name == OfName) { FoundValue = t.Value; }
                    AllValues += t.Value;
                }
            }

            return FoundValue / AllValues;
        }

        public string SoftMaxText()
        {
            var H = new List<string>();
            double Al = 0;

            const long Multi = 100000000000000000;

            foreach (var t in Neurones)
            {
                if (t.Value > 0 && !string.IsNullOrEmpty(t.Name))
                {
                    H.Add(((long)(t.Value * Multi)).ToString(Constants.Format_Integer20) + "\r" + t.Name);
                    Al += t.Value * Multi;
                }
            }

            H.Sort();

            var B = string.Empty;
            var count = 0;

            for (var Z = H.Count - 1 ; Z > H.Count - 6 ; Z--)
            {
                count++;
                if (count > 6) { break; }

                var O = H[Z].SplitBy("\r");

                B = B + O[1].ToNonCritical() + "   " + long.Parse(O[0]) / Al * 100 + " %\r\n";

            }

            return B;
        }




        public void ComputeError(Layer ChildLayer)
        {
            for (var i = 0 ; i < Neurones.Length ; i++)
            {
                var sum = ChildLayer.Neurones.Select((t, j) => t.Error * Neurones[i].Weight[j]).Sum();
                Neurones[i].Error = sum * Neurones[i].Value * (1.0 - Neurones[i].Value);
            }
        }



        internal void RefreshWeights(double LearningRate, Layer ChildLayer)
        {
            for (var i = 0 ; i < Neurones.Length ; i++)
            {
                for (var j = 0 ; j < ChildLayer.Neurones.Length ; j++)
                {
                    var dw = LearningRate * ChildLayer.Neurones[j].Error * Neurones[i].Value;
                    Neurones[i].Weight[j] += dw;
                    if (double.IsNaN(Neurones[i].Weight[j])) { DebugPrint(enFehlerArt.Fehler, "NaN entdeckt"); }
                }
            }
            for (var j = 0 ; j < ChildLayer.Neurones.Length ; j++)
            {
                BiasWeight[j] += LearningRate * ChildLayer.Neurones[j].Error * Bias[j];
            }
        }

    }
}
