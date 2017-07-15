using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    public int generation = 0;
    float generationTimer = 0.0f;

    [System.Serializable]
    public class GenerationData
    {
        public float maxFitness = 0.0f;
        public float avgFitness = 0.0f;
    }
    public List<GenerationData> generationFitness = new List<GenerationData>();

    public GameObject prefabCreature;
    public GameObject prefabFood;

    public TextMesh textGeneration;
    public TextMesh textGenerationTime;

    float foodSpawnRange = 1.75f;
    float creatureSpawnRange = 0.0f;
    int numCreaturesPerGen = 64;

    public static bool showDebugLines = true;

    HashSet<GameObject> touchedZones = new HashSet<GameObject>();

    void Start ()
    {
        Application.runInBackground = true;

        StartCoroutine(DoHandleGenerations());

        foreach(var zone in GameObject.FindObjectsOfType<Zone>())
        {
            var zoneCurr = zone;
            zoneCurr.eventTouched += () =>
            {
                zoneCurr.GetComponent<SpriteRenderer>().material.color = Color.Lerp(Color.red, Color.yellow, Mathf.Clamp01((float)zoneCurr.numTouches / 32.0f)) * 0.5f;
            };
        }
    }
	
    GameObject SpawnFoodItem()
    {
        var foodObj = GameObject.Instantiate(prefabFood);

        foodObj.transform.position = new Vector3(Random.Range(-foodSpawnRange, foodSpawnRange), Random.Range(-foodSpawnRange, foodSpawnRange), 0.0f);

        return foodObj;
    }

	IEnumerator DoHandleGenerations()
    {
        //int numNeuralNetsPassed = numCreaturesPerGen / 10;
        int numNeuralNetsPassed = 3;
        List<NeuralNetwork> passedOnNeuralNet = new List<NeuralNetwork>();

        // go thru all generations
        while (true)
        {
            textGeneration.text = string.Format("Generation: {0}", generation);

            //SpawnFoodItem();

            List<Creature> creatures = new List<Creature>();

            // create creatures
            int numCreaturesAlive = numCreaturesPerGen;
            for (int i = 0; i < numCreaturesPerGen; ++i)
            {
                var creatureObj = GameObject.Instantiate(prefabCreature);
                creatureObj.name = "Creature_" + i;
                
                //creatureObj.transform.position = new Vector3(Random.Range(-creatureSpawnRange, creatureSpawnRange), Random.Range(-creatureSpawnRange, creatureSpawnRange), 0.0f);
                creatureObj.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
                //creatureObj.transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f));

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
                /*creatureTemp.eventEatFood += (GameObject foodItem) =>
                {
                   foodItem.transform.position = new Vector3(Random.Range(-foodSpawnRange, foodSpawnRange), Random.Range(-foodSpawnRange, foodSpawnRange), 0.0f);

                    var rbFood = foodItem.GetComponent<Rigidbody2D>();
                    rbFood.velocity = Vector3.zero;
                    rbFood.angularVelocity = 0.0f; 

                    //SpawnFoodItem();
                };*/
            }

            // while simulation alive
            generationTimer = 0.0f;

            var camera = Camera.main;
           

            while (numCreaturesAlive > 0)
            {
                generationTimer += Time.deltaTime;
                textGenerationTime.text = string.Format("Time: {0:0.00}", generationTimer);

                var minX = 9999.0f;
                var minY = 9999.0f;
                var maxX = -9999.0f;
                var maxY = -9999.0f;

                foreach(var creature in creatures)
                {
                    if (creature.isDead)
                        continue;

                    var pos = creature.transform.position;

                    minX = Mathf.Min(pos.x, minX);
                    minY = Mathf.Min(pos.y, minY);
                    maxX = Mathf.Max(pos.x, maxX);
                    maxY = Mathf.Max(pos.y, maxY);

                    var centerX = (minX + maxX) * 0.5f;
                    var centerY = (minY + maxY) * 0.5f;
                    var dimenX = maxX - minX;
                    var dimenY = maxY - minY;

                    var camPos = camera.transform.position;
                    var camPosNew = new Vector3(centerX, centerY, camPos.z);

                    camera.transform.position = Vector3.Lerp(camPos, camPosNew, 0.001f);
                    camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, Mathf.Max(dimenX, dimenY) + 4.0f, 0.001f);
                }


                yield return null;
            }

            // sort creatures by fitness (descending)
            creatures.Sort((x,y) => y.fitness.CompareTo(x.fitness));

            // copy highest fitness
            var maxFitness = creatures[0].fitness;
            var avgFitness = 0.0f;
            foreach (var create in creatures)
                avgFitness += create.fitness;

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

        if ( GUI.Button(new Rect(10, 10, 100, 20), "Toggle Debug"))
        {
            showDebugLines = !showDebugLines;
        }

        //var strGeneration = string.Format("Generation: {0}", generation);
        //GUI.Label(new Rect(10, 10, 100, 20), strGeneration, myStyle);

        // gen time left
        //var strTimeLeft = string.Format("Time: {0:0.00}", generationTimer);
        //GUI.Label(new Rect(10, 10, 100, 20), strTimeLeft);
    }
}
