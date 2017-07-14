using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    public int generation = 0;
    public List<float> generationFitness = new List<float>();

    public GameObject prefabCreature;
    public GameObject prefabFood;

    void Start ()
    {
        StartCoroutine(DoHandleGenerations());
    }
	
    GameObject SpawnFoodItem()
    {
        float foodSpawnRange = 1.75f;

        var foodObj = GameObject.Instantiate(prefabFood);

        foodObj.transform.position = new Vector3(Random.Range(-foodSpawnRange, foodSpawnRange), Random.Range(-foodSpawnRange, foodSpawnRange), 0.0f);

        return foodObj;
    }

	IEnumerator DoHandleGenerations()
    {
        int numCreaturesPerGen = 12;
        float generationMaxTime = 15.0f;

        int numNeuralNetsPassed = numCreaturesPerGen / 4;
        List<NeuralNetwork> passedOnNeuralNet = new List<NeuralNetwork>();

        SpawnFoodItem();

        // go thru all generations
        while (true)
        {
            List<Creature> creatures = new List<Creature>();

            // create creatures
            int numCreaturesAlive = numCreaturesPerGen;
            for (int i = 0; i < numCreaturesPerGen; ++i)
            {
                var creatureObj = GameObject.Instantiate(prefabCreature);
                var creature = creatureObj.GetComponent<Creature>();
                creatures.Add(creature);

                // grab another neural net from previous generation
                if (passedOnNeuralNet.Count > 0)
                {
                    var sourceNeuralNet = passedOnNeuralNet[Random.Range(0,passedOnNeuralNet.Count)];

                    creature.neuralNet = new NeuralNetwork(sourceNeuralNet);
                    creature.neuralNet.Mutate();
                }

                // event - creature died :(
                creature.eventDeath += () => { --numCreaturesAlive; };

                // event - creature eats food
                creature.eventEatFood += (GameObject foodItem) =>
                {
                    SpawnFoodItem();
                };
            }

            // while simulation alive
            float generationTimeLeft = generationMaxTime;
            while (numCreaturesAlive > 0 && generationTimeLeft > 0.0f)
            {
                generationTimeLeft -= Time.deltaTime;
                yield return null;
            }

            // sort creatures by fitness (descending)
            creatures.Sort((x,y) => y.fitness.CompareTo(x.fitness));

            // copy highest fitness
            generationFitness.Add(creatures[0].fitness);

            // save neural nets to pass on
            passedOnNeuralNet.Clear();
            for (int i = 0; i < numNeuralNetsPassed; ++i)
                passedOnNeuralNet.Add(creatures[i].neuralNet);

            // delete all old creatures
            for (int i = 0; i < creatures.Count; ++i)
                GameObject.Destroy(creatures[i].gameObject);

            // next generation
            ++generation;

            yield return null;
        }
    }

    void OnGUI()
    {
        var myStyle = new GUIStyle();
        myStyle.fontSize = 24;
        myStyle.normal.textColor = Color.white;

        var strGeneration = string.Format("Generation: {0}", generation);
        GUI.Label(new Rect(10, 10, 100, 20), strGeneration, myStyle);
    }
}
