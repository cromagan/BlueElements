using System;
using System.Collections.Generic;
using static BlueBasics.Develop;
using BlueBasics;
using BlueBasics.Enums;

namespace BlueBrain
{

    // http://www.cbcity.de/tutorial-neuronale-netze-einfach-erklaert
    // https://github.com/AlexDietrich/Neuronal_Network/blob/master/Neuronal_Network/NeuronalNetwork.cs
    // https://sourceforge.net/projects/neurobox/files/0%20NeuroBox%20(all%20in%20one)/3.0%20RC1/
    // http://www.codeplanet.eu/tutorials/csharp/70-kuenstliche-neuronale-netze-in-csharp.html
    // https://github.com/PoloDev41/NeuronalNetwork


    public class FullyConnectedNetwork
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LayerCount">Die erste Angabe definiert die Anzahl der Input-Layer, die letzte Zahl die Anzahl der Output-Layer. Alle Zahlen in der Mitte definieren die Hidden-Layer</param>
        public FullyConnectedNetwork(params int[] LayerCount)
        {
            DebugPrint_NichtImplementiert();
            InitLayers(LayerCount);
        }


        /// <summary>
        /// 
        /// </summary>
        public FullyConnectedNetwork(string FileName)
        {
            Load(FileName);
        }


        public FullyConnectedNetwork(List<string> InputNames, List<string> OutputNames, params int[] HiddenLayerCount)
        {
            var Layercount2 = new int[HiddenLayerCount.Length + 2];

            for (var z = 0 ; z < HiddenLayerCount.Length ; z++)
            {
                Layercount2[z + 1] = HiddenLayerCount[z];
            }


            Layercount2[0] = InputNames.Count;
            Layercount2[Layercount2.Length - 1] = OutputNames.Count;


            InitLayers(Layercount2);

            InputLayer.SetNames(InputNames);
            OutputLayer.SetNames(OutputNames);
        }


        public Layer[] Layers;

        public Layer InputLayer
        {
            get
            {
                return Layers[0];
            }
        }

        public Layer OutputLayer
        {
            get
            {
                return Layers[Layers.Length - 1];
            }
        }



        private void InitLayers(params int[] NeuronOnLayer)
        {
            if (NeuronOnLayer.GetUpperBound(0) < 1)
            {
                DebugPrint(enFehlerArt.Fehler, "Perception kann nicht initialisiert werden.");
                return;
            }

            Layers = new Layer[NeuronOnLayer.Length];

            for (var i = 0 ; i < NeuronOnLayer.Length - 1 ; i++)
            {
                Layers[i] = new Layer(NeuronOnLayer[i], NeuronOnLayer[i + 1]);
            }

            Layers[NeuronOnLayer.Length - 1] = new Layer(NeuronOnLayer[NeuronOnLayer.Length - 1], 0);
        }



        public void Compute()
        {
            for (var i = 1 ; i < Layers.Length ; i++)
            {
                Layers[i].Compute(Layers[i - 1]);
            }
        }


        // ------------------------------------------


        //public double MeanQuadraticError = 0;




        ///// <summary>
        ///// compute the quadratic error
        ///// </summary>
        ///// <param name="expected">expected outputs</param>
        ///// <returns>quadratic error</returns>
        //public double ComputeQuadraticError(double[] expected)
        //{
        //    if (expected.Length != Layers[Layers.Length - 1].Neurones.Length)
        //        throw new ArgumentException();

        //    double sum = 0;
        //    for (int i = 0 ; i < expected.Length ; i++)
        //    {
        //        sum += Math.Pow(expected[i] - OutputLayer.Neurones[i].Value, 2);
        //    }

        //    LastQuadraticError = sum / 2;
        //    return LastQuadraticError;
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="TrainTo"></param>
        /// <param name="LearningRate"></param>
        /// <param name="ComputeBefore"></param>
        /// <returns>Gibt den LastQuadraticError VOR dem ausführen der BackPropagationGradiant zurück</returns>
        public double BackPropagationGradiant(string TrainTo, double LearningRate, bool ComputeBefore)
        {
            if (double.IsInfinity(LearningRate) || LearningRate > 0.75) { LearningRate = 0.75; }

            if (ComputeBefore) { Compute(); }

            double LastQuadraticError = 0;


            foreach (var t in OutputLayer.Neurones)
            {
                double DesiredValue = 0;
                if (TrainTo == t.Name)
                {
                    DesiredValue = 1;
                }
                else
                {
                    DesiredValue = 0;
                }
                t.Error = (DesiredValue - t.Value) * t.Value * (1.0 - t.Value);

                LastQuadraticError += Math.Pow(DesiredValue - t.Value, 2);
            }

            LastQuadraticError = LastQuadraticError / 2;

            var SM = OutputLayer.SoftMax();

            if (SM == TrainTo && OutputLayer.SoftMaxValue(SM) > 0.95) { return LastQuadraticError; }


            for (var i = Layers.Length - 2 ; i >= 0 ; i--)
            {
                Layers[i].ComputeError(Layers[i + 1]);
            }

            for (var i = 0 ; i < Layers.Length - 1 ; i++)
            {
                Layers[i].RefreshWeights(LearningRate, Layers[i + 1]);
            }

            return LastQuadraticError;
        }

        public void Save(string Filename)
        {
            var ToSave = new List<string>();
            ToSave.Add("Identifer;NeuronalNetworkV1.0");
            ToSave.Add("Info;Neuronal Network (c) Christian Peter");

            ToSave.Add("LayerCount;" + Layers.Length);

            for (var i = 0 ; i < Layers.Length ; i++)
            {
                ToSave.Add("NeuronesOnLayer;" + Layers[i].Neurones.Length);
                ToSave.Add("NeuronesOnNextLayer;" + Layers[i].Neurones[0].Weight.Length);

                for (var j = 0 ; j < Layers[i].Neurones.Length ; j++)
                {
                    ToSave.Add("NeuronName;" + Layers[i].Neurones[j].Name.ToNonCritical());

                    for (var w = 0 ; w < Layers[i].Neurones[j].Weight.Length ; w++)
                    {
                        ToSave.Add("Weight;" + Layers[i].Neurones[j].Weight[w]);
                        if (double.IsNaN(Layers[i].Neurones[j].Weight[w])) { DebugPrint(enFehlerArt.Fehler, "NaN kann nicht gespeichert werden"); }
                    }
                }

                for (var j = 0 ; j < Layers[i].Bias.Length ; j++)
                {
                    ToSave.Add("NeuronBias;" + Layers[i].Bias[j]);
                    if (double.IsNaN(Layers[i].Bias[j])) { DebugPrint(enFehlerArt.Fehler, "NaN kann nicht gespeichert werden"); }

                    ToSave.Add("NeuronBiasWeight;" + Layers[i].BiasWeight[j]);
                    if (double.IsNaN(Layers[i].BiasWeight[j])) { DebugPrint(enFehlerArt.Fehler, "NaN kann nicht gespeichert werden"); }
                }
            }
            ToSave.Add("EOF;-");
            ToSave.Save(Filename, false);
        }


        private void Load(string Filename)
        {

            var Loaded = new List<string>();
            Loaded.Load(Filename);

            var NeuronesOnLayer = 0;

            var LayerNo = -1;
            var NeuronNo = -1;
            var WeightNo = -1;
            var BiasNo = -1;


            foreach (var thisString in Loaded)
            {
                var t = thisString.SplitBy(";");

                switch (t[0])
                {
                    case "Info":
                    case "Identifer":
                        break;
                    case "NeuronesOnLayer":
                        NeuronesOnLayer = int.Parse(t[1]);
                        break;

                    case "NeuronesOnNextLayer":
                        var NeuronesOnNextLayer = int.Parse(t[1]);
                        LayerNo++;
                        Layers[LayerNo] = new Layer(NeuronesOnLayer, NeuronesOnNextLayer);
                        NeuronNo = -1;
                        BiasNo = -1;
                        break;

                    case "LayerCount":
                        Layers = new Layer[int.Parse(t[1])];
                        LayerNo = -1;
                        break;

                    case "NeuronName":
                        NeuronNo++;

                        if (t.Length == 2) { Layers[LayerNo].Neurones[NeuronNo].Name = t[1].FromNonCritical(); }

                        if (LayerNo == 0 || LayerNo == Layers.Length - 1)
                        {
                            if (string.IsNullOrEmpty(Layers[LayerNo].Neurones[NeuronNo].Name)) { DebugPrint(enFehlerArt.Fehler, "Neuron ohne Namen geladen"); }
                        }

                        WeightNo = -1;
                        break;

                    case "Weight":
                        WeightNo++;
                        Layers[LayerNo].Neurones[NeuronNo].Weight[WeightNo] = double.Parse(t[1]);
                        if (double.IsNaN(Layers[LayerNo].Neurones[NeuronNo].Weight[WeightNo])) { DebugPrint(enFehlerArt.Fehler, "NaN geladen"); }
                        break;

                    case "NeuronBias":
                        BiasNo++;
                        Layers[LayerNo].Bias[BiasNo] = double.Parse(t[1]);
                        if (double.IsNaN(Layers[LayerNo].Bias[BiasNo])) { DebugPrint(enFehlerArt.Fehler, "NaN geladen"); }
                        break;

                    case "NeuronBiasWeight":
                        Layers[LayerNo].BiasWeight[BiasNo] = double.Parse(t[1]);
                        if (double.IsNaN(Layers[LayerNo].BiasWeight[BiasNo])) { DebugPrint(enFehlerArt.Fehler, "NaN geladen"); }
                        break;

                    case "EOF":
                        return;

                    default:
                        DebugPrint(enFehlerArt.Fehler, "Tag nicht bekannt: " + thisString);
                        break;
                }
            }
        }
    }
}
