using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CreatureFrog : CreatureBase
{
    Coroutine threadMoving = null;

    public ParticleSystem particles;

    [ReadOnly]
    public GridPos gridPos = new GridPos(0, 0);

    int scanRangeForward = 5; // +z
    int scanRangeBackwards = 3; // -z
    int scanRangeSide = 3; // +/- x

    int maxGridZ = 2;

    public MeshRenderer mesh;

    [ReadOnly]
    public int numTurnsLeft = 5;
    int numTurnsReplenishedMoveForward = 10;

    void Start ()
    {
        gridPos.x = FrogWorld.numGridsX / 2;
        gridPos.z = maxGridZ;

        // add death state
        eventDeath += () =>
        {
            mesh.material.SetColor("_Color", Color.white * 0.3f);

            transform.DOKill(true);
            //transform.DOPunchRotation(Vector3.one * 15.0f, 0.4f, 10);
            transform.DORotateQuaternion(Quaternion.Euler(90.0f, 0.0f, 0.0f) * transform.rotation, 0.2f).SetEase(Ease.OutBack);

            var newScale = transform.localScale;
            newScale.Scale(new Vector3(0.75f, 0.75f, 0.75f));
            transform.DOScale(newScale, 0.2f).SetEase(Ease.OutBack);

            this.StopAndNullify(ref threadMoving);
        };

        threadMoving = StartCoroutine(HandleMovement());

        transform.localScale = Vector3.one * FrogWorld.gridSize;
        transform.position = FrogWorld.GridToWorldPos(gridPos);
    }

    public override void CreateNeuralNetwork()
    {
        // scan grid range
        int scanDimenX = scanRangeSide * 2 + 1;
        int scanDimenY = scanRangeForward + scanRangeBackwards + 1;
        int numInputs = scanDimenX * scanDimenY;

        // the number of buttons
        int numOutputs = 4;

        int[] layerSizes = new int[] { numInputs, 7, numOutputs };
        InitNeuralNetworkHelper(layerSizes);
    }

    void UpdateNeuralNetOutput()
    {
        var inputIndex = 0;
        var frogWorld = FrogWorld.Instance;

        for (int z = -scanRangeBackwards; z <= scanRangeForward; ++z)
        {
            for (int x = -scanRangeSide; x <= scanRangeSide; ++x)
            {
                var scanGridPos = (new GridPos(x, z)) + gridPos;

                // not valid
                if (!frogWorld.IsValidGridPos(scanGridPos))
                    neuralNetInput[inputIndex] = 2;

                // obstacle
                else if (frogWorld.HasObstacle(scanGridPos))
                    neuralNetInput[inputIndex] = 1;

                // open!
                else
                    neuralNetInput[inputIndex] = 0;

                ++inputIndex;
            }
        }

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
    }

    void OnCollisionEnter2D(Collision2D collisionObj)
    {
        if (collisionObj.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            TriggerDeath();
        }
    }*/

    IEnumerator HandleMovement()
    {
        FrogWorld frogWorld = FrogWorld.Instance;
        float gridSize = FrogWorld.gridSize;

        while (true)
        {
            UpdateNeuralNetOutput();

            GridPos gridDelta = new GridPos(0, 0);

            // wait for input
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

                /*if (Random.Range(0,2) == 0)
                    gridDelta.x = Random.Range(0, 3) - 1;
                else
                    gridDelta.z = Random.Range(0, 3) - 1;*/

                // neural network input
                var maxIndex = -1;
                var maxVal = -99.0f;
                for(int i = 0; i < 4; ++i)
                {
                    var val = neuralNet.GetOutputData(i);

                    if (Mathf.Abs(val) > maxVal)
                    {
                        maxVal = Mathf.Abs(val);
                        maxIndex = i;
                    }
                }

                var inputValThreshold = 0.2f;
                if (maxVal > inputValThreshold)
                {
                    if (maxIndex == 0)
                            gridDelta.x += 1;
                    if (maxIndex == 1)
                        gridDelta.x -= 1;
                    if (maxIndex == 2)
                        gridDelta.z -= 1;
                    if (maxIndex == 3)
                        gridDelta.z += 1;
                }

                if (gridDelta.x != 0 || gridDelta.z != 0)
                    break;

                yield return null;
            }

            // new grid pos
            GridPos newGridPos = gridPos + gridDelta;
            Vector3 newPos = new Vector3(newGridPos.x * gridSize, gridSize * 0.5f, newGridPos.z * gridSize);

            // turn
            var turnAngle = Mathf.Rad2Deg * Mathf.Atan2(-gridDelta.z, gridDelta.x) + 90f;
            transform.DORotate(new Vector3(0.0f, turnAngle, 0.0f), 0.1f).SetEase(Ease.OutBack);

            // hits wall
            if (frogWorld.HasObstacle(newGridPos))
            {
                --numTurnsLeft;

                var partPos = Vector3.Lerp(transform.position, newPos, 0.25f);
                yield return transform.DOMove(partPos, 0.05f).SetLoops(2, LoopType.Yoyo).WaitForCompletion();
            }
            else // move to empty spot
            {
                particles.Emit(2);

                // move there
                yield return transform.DOMove(newPos, 0.1f).SetEase(Ease.OutBack).WaitForCompletion();

                // moving forward
                if (newGridPos.z > maxGridZ)
                {
                    numTurnsLeft = Mathf.Max(numTurnsLeft, numTurnsReplenishedMoveForward);

                    maxGridZ = newGridPos.z;

                    fitness += 1.0f;
                }
                else // not moving forwad
                {
                    --numTurnsLeft;
                }

                gridPos = newGridPos;
            }

            // no more turns left
            if (numTurnsLeft < 0)
                TriggerDeath();

            yield return null;
        }
    }
}
