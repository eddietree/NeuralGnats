using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBase : MonoBehaviour
{
    public bool isDead = false;

    public NeuralNetwork neuralNet;
    public float[] neuralNetInput;

    public float fitness = 0.0f;
    protected float lifeSpan = 0.0f;

    virtual public void InitNeuralNetwork()
    {

    }

    public void CopyNeuralNetwork(NeuralNetwork other)
    {
        neuralNet = new NeuralNetwork(other);
    }
}
