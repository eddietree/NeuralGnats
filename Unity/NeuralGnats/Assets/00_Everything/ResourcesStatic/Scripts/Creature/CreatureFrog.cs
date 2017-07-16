using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureFrog : CreatureBase
{
    Coroutine threadMoving = null;

    void Start ()
    {
        // add death state
        eventDeath += () =>
        {
           
        };

        this.StopAndNullify(ref threadMoving);
        threadMoving = StartCoroutine(HandleMovement());
    }

    public override void CreateNeuralNetwork()
    {
        int numInputs = 4;
        int numOutputs = 4;

        int[] layerSizes = new int[] { numInputs, 7, numOutputs };
        InitNeuralNetworkHelper(layerSizes);
    }

    void UpdateNeuralNetOutput()
    {
        // TODO feed input
        neuralNetInput[0] = 0.0f;

        neuralNet.FeedForward(neuralNetInput);
    }

    /*void OnTriggerEnter2D(Collider2D collisionObj)
    {
        if (collisionObj.gameObject.layer == LayerMask.NameToLayer("Zone"))
        {
            var collisionGameObj = collisionObj.gameObject;

            // doesn't touch?
            if (!touchedZones.Contains(collisionGameObj))
            {
                var zone = collisionGameObj.GetComponent<Zone>();

                if (zone.eventTouched != null)
                    zone.eventTouched();

                lifeSpan = Mathf.Min(lifeSpan + lifeSpanEat, lifeSpanMax);

                touchedZones.Add(collisionGameObj);
                fitness += 1.0f;
            }
        }
    }*/

    void OnCollisionEnter2D(Collision2D collisionObj)
    {
        if (collisionObj.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            TriggerDeath();
        }
    }

    IEnumerator HandleMovement()
    {
        while (true)
        {
            UpdateNeuralNetOutput();

            /*lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0.0f)
                TriggerDeath();*/

            yield return null;
        }
    }
}
