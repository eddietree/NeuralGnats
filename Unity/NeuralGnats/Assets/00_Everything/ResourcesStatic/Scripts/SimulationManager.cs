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

    Coroutine threadRunSim = null;

    void Start ()
    {
        Application.runInBackground = true;
        threadRunSim = StartCoroutine(DoHandleGenerations());
    }

    public void RestartSimulation()
    {
        generation = 0;
        generationFitness.Clear();
        CleanUpSimulation();

        this.StopAndNullify(ref threadRunSim);
        threadRunSim = StartCoroutine(DoHandleGenerations());
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

            yield return new WaitForSeconds(0.3f);

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

            CleanUpSimulation();

            // next generation
            ++generation;

            yield return null;
        }
    }

    void CleanUpSimulation()
    {
        // delete all old creatures
        for (int i = 0; i < creatures.Count; ++i)
            GameObject.Destroy(creatures[i].gameObject);
    }

    public Font debugFont;

    void OnGUI()
    {
        // gen time left
        int numCreaturesAlive = 0;
        float maxFitness = 0.0f;
        float avgFitness = 0.0f;
        foreach (var creature in creatures)
        {
            if (!creature.isDead)
                ++numCreaturesAlive;

            maxFitness = Mathf.Max(creature.fitness, maxFitness);
            avgFitness += creature.fitness;
        }
        avgFitness = avgFitness / creatures.Count;

        var myStyle = new GUIStyle();
        myStyle.font = debugFont;
        myStyle.normal.textColor = Color.white;

        // headline
        myStyle.fontSize = 48;
        var strGeneration = string.Format("Generation {0}", generation);
        GUI.Label(new Rect(10, 10, 100, 20), strGeneration, myStyle);

        myStyle.fontSize = 18;

        // max fitness
        GUI.Label(new Rect(10, 60, 100, 20), string.Format("Max Fitness: {0} (Avg: {1:0.00})", maxFitness, avgFitness), myStyle);

        // creatures left
        GUI.Label(new Rect(10, 80, 100, 20), string.Format("Creatures: {0} / {1}", numCreaturesAlive, creatures.Count), myStyle);

        // gen time left
        GUI.Label(new Rect(10, 100, 100, 20), string.Format("Time: {0:0.00} s", generationTimer), myStyle);

        // print previous generations
        myStyle.fontSize = 18;
        for (int i = 0; i < Mathf.Min(15, generationFitness.Count); ++i)
        {
            var genIndex = generationFitness.Count - 1 - i;

            var data = generationFitness[genIndex];
            var generationStr = string.Format("Gen {0}: {1}, Avg {2:0.00}", genIndex, data.maxFitness, data.avgFitness);
            GUI.Label(new Rect(10, 140 + (generationFitness.Count - genIndex) * 20, 100, 20), generationStr, myStyle);
        }
    }
}
