using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron
{
    public float[] weights = null;
    public float val = 0.0f;

    public Neuron()
    {}

    public void InitWeights(int numWeights)
    {
        // init weights
        weights = new float[numWeights];
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
    NeuronLayer[] neuronLayers;

    public NeuralNetwork(NeuralNetwork other)
    {
        neuronLayers = new NeuronLayer[other.neuronLayers.Length];

        // copy layers
        for (int iLayer = 0; iLayer < other.neuronLayers.Length; ++iLayer)
        {
            var otherNeuronLayer = other.neuronLayers[iLayer];

            var currNeuronLayer = new NeuronLayer();
            currNeuronLayer.InitNeurons(otherNeuronLayer.neurons.Length);

            // copy neurons
            for(int iNeuron = 0; iNeuron < currNeuronLayer.neurons.Length; ++iNeuron)
            {
                var otherNeuron = otherNeuronLayer.neurons[iNeuron];
                
                // copy other neurons
                if (otherNeuron.weights != null)
                {
                    var currNeuron = currNeuronLayer.neurons[iNeuron];
                    currNeuron.InitWeights(otherNeuron.weights.Length);

                    // copy weights
                    for (int iWeight = 0; iWeight < otherNeuron.weights.Length; ++iWeight)
                    {
                        currNeuron.weights[iWeight] = otherNeuron.weights[iWeight];
                    }
                }
            }

            neuronLayers[iLayer] = currNeuronLayer;
        }
    }

    public NeuralNetwork(int[] layerSizes)
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
                    neuron.InitWeights(prevNeuronLayer.neurons.Length);
                }
            }

            neuronLayers[iLayer] = currNeuronLayer;
        }
    }

    float sigmoid(float val)
    {
        return 1.0f / (1.0f + Mathf.Exp(-val));
    }

    public void Mutate()
    {
        for (int iLayer = 0; iLayer < neuronLayers.Length; ++iLayer)
        {
            var currNeuronLayer = neuronLayers[iLayer];

            // copy neurons
            for (int iNeuron = 0; iNeuron < currNeuronLayer.neurons.Length; ++iNeuron)
            {
                var currNeuron = currNeuronLayer.neurons[iNeuron];

                if (currNeuron.weights == null)
                    continue;

                for (int iWeight = 0; iWeight < currNeuron.weights.Length; ++iWeight)
                {
                    var weight = currNeuron.weights[iWeight];
                    float randomNumber = UnityEngine.Random.Range(0f, 100f);

                    if (randomNumber <= 2f)
                    { //if 1
                      //flip sign of weight
                        weight *= -1f;
                    }
                    else if (randomNumber <= 4f)
                    { //if 2
                      //pick random weight between -1 and 1
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if (randomNumber <= 6f)
                    { //if 3
                      //randomly increase by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 8f)
                    { //if 4
                      //randomly decrease by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }

                    currNeuron.weights[iWeight] = weight;
                }
            }
        }
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
            var prevNeuronLayer = neuronLayers[iNeuronLayer - 1];

            var currNeuronLayer = neuronLayers[iNeuronLayer];
            var currNeurons = currNeuronLayer.neurons;

            for(int iNeuron = 0; iNeuron < currNeurons.Length; ++iNeuron)
            {
                var neuron = currNeurons[iNeuron];

                float result = 0.0f;

                for(int iWeight = 0; iWeight < neuron.weights.Length; ++iWeight)
                {
                    var prevNeuronVal = prevNeuronLayer.neurons[iWeight].val;
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
