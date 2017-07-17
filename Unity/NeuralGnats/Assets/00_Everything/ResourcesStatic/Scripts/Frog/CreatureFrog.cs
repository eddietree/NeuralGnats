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
        gridPos.x = FrogWorld.numGridsX / 2;
        gridPos.z = 2;

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
        FrogWorld frogWorld = FrogWorld.Instance;

        while (true)
        {
            UpdateNeuralNetOutput();

            GridPos gridDelta = new GridPos(0, 0);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    --gridDelta.x;

                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    ++gridDelta.x;

                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                    --gridDelta.z;

                else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
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
            Vector3 newPos = new Vector3(newGridPos.x * gridSize, gridSize * 0.5f, newGridPos.z * gridSize);

            // turn
            var turnAngle = Mathf.Rad2Deg * Mathf.Atan2(-gridDelta.z, gridDelta.x) + 90f;

            transform.DORotate(new Vector3(0.0f, turnAngle, 0.0f), 0.1f).SetEase(Ease.OutBack);

            // hits wall
            if (frogWorld.HasObstacle(newGridPos))
            {
                var partPos = Vector3.Lerp(transform.position, newPos, 0.25f);
                yield return transform.DOMove(partPos, 0.05f).SetLoops(2, LoopType.Yoyo).WaitForCompletion();
            }
            else // move to empty spot
            {
                // move there
                yield return transform.DOMove(newPos, 0.1f).SetEase(Ease.OutBack).WaitForCompletion();

                gridPos = newGridPos;
            }

            /*lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0.0f)
                TriggerDeath();*/

            yield return null;
        }
    }
}
