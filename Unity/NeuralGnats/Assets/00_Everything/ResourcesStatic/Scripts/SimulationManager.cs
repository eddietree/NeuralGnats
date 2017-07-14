using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    public int generation = 0;
    float generationTimeLeft = 0.0f;
    public List<float> generationFitness = new List<float>();

    public GameObject prefabCreature;
    public GameObject prefabFood;

    float foodSpawnRange = 1.75f;

    void Start ()
    {
        StartCoroutine(DoHandleGenerations());
    }
	
    GameObject SpawnFoodItem()
    {
        var foodObj = GameObject.Instantiate(prefabFood);

        foodObj.transform.position = new Vector3(Random.Range(-foodSpawnRange, foodSpawnRange), Random.Range(-foodSpawnRange, foodSpawnRange), 0.0f);

        return foodObj;
    }

	IEnumerator DoHandleGenerations()
    {
        int numCreaturesPerGen = 24;
        float generationMaxTime = 50.0f;
        var creatureSpawnRange = 1.0f;

        int numNeuralNetsPassed = numCreaturesPerGen / 6;
        List<NeuralNetwork> passedOnNeuralNet = new List<NeuralNetwork>();

        // go thru all generations
        while (true)
        {
            SpawnFoodItem();

            List<Creature> creatures = new List<Creature>();

            // create creatures
            int numCreaturesAlive = numCreaturesPerGen;
            for (int i = 0; i < numCreaturesPerGen; ++i)
            {
                var creatureObj = GameObject.Instantiate(prefabCreature);
                
                creatureObj.transform.position = new Vector3(Random.Range(-creatureSpawnRange, creatureSpawnRange), Random.Range(-creatureSpawnRange, creatureSpawnRange), 0.0f);
                creatureObj.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f));

                var creature = creatureObj.GetComponent<Creature>();
                creatures.Add(creature);

                // grab another neural net from previous generation
                if (passedOnNeuralNet.Count > 0)
                {
                    var sourceNeuralNet = passedOnNeuralNet[i % passedOnNeuralNet.Count];

                    creature.neuralNet = new NeuralNetwork(sourceNeuralNet);
                    creature.neuralNet.Mutate();
                }

                // event - creature died :(
                var creatureTemp = creature;
                creatureTemp.eventDeath += () => 
                {
                    --numCreaturesAlive;

                    creatureTemp.eventEatFood = null;
                    creatureTemp.eventDeath = null;
                };

                // event - creature eats food
                creatureTemp.eventEatFood += (GameObject foodItem) =>
                {
                   foodItem.transform.position = new Vector3(Random.Range(-foodSpawnRange, foodSpawnRange), Random.Range(-foodSpawnRange, foodSpawnRange), 0.0f);

                    var rbFood = foodItem.GetComponent<Rigidbody2D>();
                    rbFood.velocity = Vector3.zero;
                    rbFood.angularVelocity = 0.0f; 

                    //SpawnFoodItem();
                };
            }

            // while simulation alive
            generationTimeLeft = generationMaxTime;
            //while (numCreaturesAlive > 0 && generationTimeLeft > 0.0f)
            while (numCreaturesAlive > 0)
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
            {
                creatures[i].eventDeath = null;
                creatures[i].eventEatFood = null;

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

    void OnGUI()
    {
        var myStyle = new GUIStyle();
        myStyle.fontSize = 24;
        myStyle.normal.textColor = Color.white;

        var strGeneration = string.Format("Generation: {0}", generation);
        GUI.Label(new Rect(10, 10, 100, 20), strGeneration, myStyle);

        // gen time left
        var strTimeLeft = string.Format("Time: {0}", generationTimeLeft);
        GUI.Label(new Rect(10, 40, 100, 20), strTimeLeft);
    }
}
