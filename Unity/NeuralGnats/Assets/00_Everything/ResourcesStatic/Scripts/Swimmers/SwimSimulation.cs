using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimSimulation : SimulationBase
{
    public GameObject prefabFood;

    float foodSpawnRange = 1.75f;
    float creatureSpawnRange = 0.1f;

    void Start ()
    {
        InitZones();
    }

    override public void OnUpdateSimulation()
    {
        base.OnUpdateSimulation();

        var minX = 9999.0f;
        var minY = 9999.0f;
        var maxX = -9999.0f;
        var maxY = -9999.0f;

        var camera = Camera.main;
        var creatures = SimulationManager.Instance.creatures;

        foreach (var creature in creatures)
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
    }

    override public void OnCreatureCreated(CreatureBase creature)
    {
        creature.transform.position = new Vector3(Random.Range(-creatureSpawnRange, creatureSpawnRange), Random.Range(-creatureSpawnRange, creatureSpawnRange), 0.0f);

        creature.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
    }

    void InitZones()
    {
        foreach (var zone in GameObject.FindObjectsOfType<Zone>())
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
}
