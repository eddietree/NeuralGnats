using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron
{
    public float[] weights = null;
    public NeuronLayer prevLayer = null;
    public float val = 0.0f;

    public Neuron()
    {}

    public void ConnectWithPrevLayer(NeuronLayer _prevLayer)
    {
        prevLayer = _prevLayer;

        // init rates
        weights = new float[_prevLayer.neurons.Length];
        for(int i = 0; i < weights.Length; ++i)
            weights[i] = UnityEngine.Random.Range(-0.5f, 0.5f);
    }
}

public class NeuronLayer
{
    public Neuron[] neurons;

    public void InitNeurons(int neuronCount)
    {
        neurons = new Neuron[neuronCount];

        for (int i = 0; i < neuronCount; ++i)
            neurons[i] = new Neuron();
    }
}

public class NeuralNetwork
{
    // 1st val = input
    // last val = output (left + right thurster)
    static int[] layerSizes = new int[] { 3, 10, 10, 2 };

    NeuronLayer[] neuronLayers;
    //float fitness;

    public NeuralNetwork()
    {
        neuronLayers = new NeuronLayer[layerSizes.Length];

        for (int iLayer = 0; iLayer < layerSizes.Length; ++iLayer)
        {
            var numNeuronsInLayer = layerSizes[iLayer];

            // create neurons
            var currNeuronLayer = new NeuronLayer();
            currNeuronLayer.InitNeurons(numNeuronsInLayer);

            // connect to previous neurons
            if (iLayer > 0)
            {
                var prevNeuronLayer = neuronLayers[iLayer - 1];

                for (int iNeuron = 0; iNeuron < numNeuronsInLayer; ++iNeuron)
                {
                    var neuron = currNeuronLayer.neurons[iNeuron];
                    neuron.ConnectWithPrevLayer(prevNeuronLayer);
                }
            }

            neuronLayers[iLayer] = currNeuronLayer;
        }
    }

    float sigmoid(float val)
    {
        return 1.0f / (1.0f + Mathf.Exp(-val));
    }

    public void FeedForward(float[] inputs, float[] outputs)
    {
        // assert input is same 
        Debug.Assert(inputs.Length == neuronLayers[0].neurons.Length);

        // assert output is same 
        Debug.Assert(outputs.Length == neuronLayers[neuronLayers.Length - 1].neurons.Length);

        // input to first neuron layer
        var firstNeuronLayer = neuronLayers[0];
        for (int i = 0; i < inputs.Length; ++i)
            firstNeuronLayer.neurons[i].val = inputs[i];

        // feed-forward calculations
        for (int iNeuronLayer = 1; iNeuronLayer < neuronLayers.Length; ++iNeuronLayer)
        {
            var currNeuronLayer = neuronLayers[iNeuronLayer];
            var currNeurons = currNeuronLayer.neurons;

            for(int iNeuron = 0; iNeuron < currNeurons.Length; ++iNeuron)
            {
                var neuron = currNeurons[iNeuron];

                float result = 0.0f;

                for(int iWeight = 0; iWeight < neuron.weights.Length; ++iWeight)
                {
                    var prevNeuronVal = neuron.prevLayer.neurons[iWeight].val;
                    var weight = neuron.weights[iWeight];

                    result += weight * prevNeuronVal;
                }

                neuron.val = sigmoid(result);
            }
        }

        var lastNeuronLayer = neuronLayers[neuronLayers.Length-1];
        for (int i = 0; i < outputs.Length; ++i)
        {
            outputs[i] = lastNeuronLayer.neurons[i].val;
        }
    }
}
