using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CreatureFrog : CreatureBase
{
    Coroutine threadMoving = null;

    [ReadOnly]
    public GridPos gridPos = new GridPos(0, 0);

    void Start ()
    {
        Vector3 test = Vector3.one;
        var t = test + Vector3.zero;

        // add death state
        eventDeath += () =>
        {
           
        };

        threadMoving = StartCoroutine(HandleMovement());

        transform.localScale = Vector3.one * FrogWorld.gridSize;
        transform.position = FrogWorld.GridToWorldPos(gridPos);
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
        //neuralNetInput[0] = 0.0f;

        //neuralNet.FeedForward(neuralNetInput);
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

            GridPos gridDelta = new GridPos(0, 0);

            while (true)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    --gridDelta.x;

                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    ++gridDelta.x;

                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    --gridDelta.z;

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    ++gridDelta.z;

                // TODO: neural network input
                //neuralNet.GetOutputData(0);

                if (gridDelta.x != 0 || gridDelta.z != 0)
                    break;

                yield return null;
            }

            float gridSize = FrogWorld.gridSize;

            // 
            GridPos newGridPos = gridPos + gridDelta;

            // move to new pos
            Vector3 newPos = new Vector3(newGridPos.x * gridSize, gridSize * 0.5f, newGridPos.z * gridSize);
            yield return transform.DOMove(newPos, 0.15f).SetEase(Ease.OutBack).WaitForCompletion();

            gridPos = newGridPos;

            /*lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0.0f)
                TriggerDeath();*/

            yield return null;
        }
    }
}
