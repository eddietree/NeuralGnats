using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBase : MonoBehaviour
{
    [ReadOnly]
    public bool isDead = false;

    public NeuralNetwork neuralNet;

    [ReadOnly]
    public float[] neuralNetInput;

    [ReadOnly]
    public float fitness = 0.0f;

    [ReadOnly]
    protected float lifeSpan = 0.0f;

    public delegate void DeathEvent();
    public DeathEvent eventDeath;

    virtual public void CreateNeuralNetwork()
    {
        //int[] layerSizes = new int[] { numInputs, 7, numOutputs };
        //InitNeuralNetworkHelper(layerSizes);
    }

    protected void TriggerDeath()
    {
        Debug.AssertFormat(!isDead, "Error, '{0}' already dead!", gameObject.name);

        if (isDead)
            return;

        if (eventDeath != null)
            eventDeath();

        eventDeath = null;
        isDead = true;
    }

    protected void InitNeuralNetworkHelper(int[] layerSizes)
    {
        int numInputs = layerSizes[0];

        // inits
        neuralNetInput = new float[numInputs];
        for (int i = 0; i < neuralNetInput.Length; ++i)
            neuralNetInput[i] = 0.0f;

        neuralNet = new NeuralNetwork(layerSizes);
    }

    public void CopyNeuralNetwork(NeuralNetwork other)
    {
        neuralNet = new NeuralNetwork(other);
    }
}
