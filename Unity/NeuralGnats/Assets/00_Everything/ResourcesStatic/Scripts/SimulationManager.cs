using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenerationData
{
    public float maxFitness = 0.0f;
    public float avgFitness = 0.0f;
}

public class SimulationManager : SingletonMonoBehaviourOnDemand<SimulationManager>
{
    [ReadOnly]
    public int generation = 0;

    [ReadOnly]
    float generationTimer = 0.0f;

    [ReadOnly]
    public List<GenerationData> generationFitness = new List<GenerationData>();

    [ReadOnly]
    public List<CreatureBase> creatures = new List<CreatureBase>();

    public SimulationBase simulation;
    public GameObject prefabCreature;
    public int numCreaturesPerGen = 128;

    void Start ()
    {
        Application.runInBackground = true;
        StartCoroutine(DoHandleGenerations());
    }

	IEnumerator DoHandleGenerations()
    {
        //int numNeuralNetsPassed = numCreaturesPerGen / 10;
        int numNeuralNetsPassed = 4;
        List<NeuralNetwork> passedOnNeuralNet = new List<NeuralNetwork>();

        prefabCreature.SetActive(false);

        // go thru all generations
        while (true)
        {
            creatures.Clear();

            // create creatures
            int numCreaturesAlive = numCreaturesPerGen;
            for (int i = 0; i < numCreaturesPerGen; ++i)
            {
                var creatureObj = GameObject.Instantiate(prefabCreature);
                creatureObj.name = string.Format("Creature_{0}_{1}", generation, i);
                creatureObj.SetActive(true);

                // add creature
                var creature = creatureObj.GetComponent<CreatureBase>();
                creature.CreateNeuralNetwork();
                creatures.Add(creature);

                // simluation
                if (simulation)
                    simulation.OnCreatureCreated(creature);

                // grab another neural net from previous generation
                if (passedOnNeuralNet.Count > 0)
                {
                    var sourceNeuralNet = passedOnNeuralNet[i % passedOnNeuralNet.Count];

                    creature.CopyNeuralNetwork(sourceNeuralNet);
                    creature.neuralNet.Mutate();
                }

                // event - creature died :(
                var creatureTemp = creature;
                creatureTemp.eventDeath += () => 
                {
                    --numCreaturesAlive;
                };
            }

            if (simulation)
                simulation.OnStartSimulation();

            // while simulation alive
            generationTimer = 0.0f;
            while (numCreaturesAlive > 0)
            {
                generationTimer += Time.deltaTime;

                if (simulation)
                    simulation.OnUpdateSimulation();

                yield return null;
            }

            if (simulation)
                simulation.OnStopSimulation();

            // sort creatures by fitness (descending)
            creatures.Sort((x,y) => y.fitness.CompareTo(x.fitness));

            // copy highest fitness
            var maxFitness = creatures[0].fitness;
            var avgFitness = 0.0f;
            foreach (var create in creatures)
                avgFitness += create.fitness;

            // set fitness data
            var fitnessData = new GenerationData();
            fitnessData.maxFitness = maxFitness;
            fitnessData.avgFitness = (float)avgFitness / (float)creatures.Count;

            generationFitness.Add(fitnessData);

            // save neural nets to pass on
            passedOnNeuralNet.Clear();
            for (int i = 0; i < numNeuralNetsPassed; ++i)
            {
                var neuralNetNew = new NeuralNetwork(creatures[i].neuralNet);
                passedOnNeuralNet.Add(neuralNetNew);
            }

            // delete all old creatures
            for (int i = 0; i < creatures.Count; ++i)
            {
                GameObject.Destroy(creatures[i].gameObject);
            }

            // destroy all food items
            var foods = GameObject.FindObjectsOfType<Food>();
            for (int i = foods.Length - 1; i >= 0; --i)
                GameObject.Destroy(foods[i].gameObject);

            // next generation
            ++generation;

            yield return null;
        }
    }

    public Font debugFont;

    void OnGUI()
    {
        var myStyle = new GUIStyle();
        myStyle.font = debugFont;
        myStyle.fontSize = 32;
        myStyle.normal.textColor = Color.white;

        var strGeneration = string.Format("Generation: {0}", generation);
        GUI.Label(new Rect(10, 10, 100, 20), strGeneration, myStyle);

        // gen time left
        myStyle.fontSize = 18;
        var strTimeLeft = string.Format("Time: {0:0.00}", generationTimer);
        GUI.Label(new Rect(10, 40, 100, 20), strTimeLeft, myStyle);

        // print previous generations
        for(int i = 0; i < Mathf.Min(15, generationFitness.Count); ++i)
        {
            var genIndex = generationFitness.Count - 1 - i;

            var data = generationFitness[genIndex];
            var generationStr = string.Format("Gen {0}: {1:0.00}", genIndex, data.avgFitness);
            GUI.Label(new Rect(10, 70 + (generationFitness.Count - genIndex) * 20, 100, 20), generationStr, myStyle);
        }

    }
}
