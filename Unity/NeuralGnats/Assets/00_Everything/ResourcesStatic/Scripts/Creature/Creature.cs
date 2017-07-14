﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour {

    public GameObject thrusterProto;

    public Transform thrusterLeftMount;
    public Transform thrusterRightMount;
    CreatureThruster thrusterLeft;
    CreatureThruster thrusterRight;
    Rigidbody2D rb;

    bool isDead = false;

    const int numFeelers = 5;
    const float angleSpreadDegrees = 110.0f;
    const float feelerDist = 2.5f;

    public float[] feelerDanger = new float[numFeelers];
    public float[] feelerHunger = new float[numFeelers];

    public float[] neuralNetInput;
    public float[] neuralNetOutput;

    public delegate void DeathEvent(Creature creature);
    public DeathEvent eventDeath;

    NeuralNetwork neuralNet;

    public float fitness = 0.0f;

    Coroutine threadSteering;
    
    void Start ()
    {
        InitNeuralNetwork();

        rb = GetComponent<Rigidbody2D>();

        thrusterLeft = CreateThruster(thrusterLeftMount);
        thrusterRight = CreateThruster(thrusterRightMount);
        thrusterProto.SetActive(false);

        Reset();
	}

    private void Reset()
    {
        if (threadSteering != null)
        {
            StopCoroutine(threadSteering);
            threadSteering = null;
        }

        threadSteering = StartCoroutine(HandleSteering());
        isDead = false;

        fitness = 0.0f;
    }

    void OnDeath()
    {
        if (threadSteering != null)
        {
            StopCoroutine(threadSteering);
            threadSteering = null;
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0.0f;

        isDead = true;
        GetComponent<BoxCollider2D>().enabled = false;
    }

    void InitNeuralNetwork()
    {
        // feelers, angle, velocity, angular velocity
        int numInputs = feelerDanger.Length + feelerHunger.Length + 3;
        int numOutputs = 2;

        neuralNetInput = new float[numInputs];
        neuralNetOutput = new float[numOutputs];

        for (int i = 0; i < neuralNetInput.Length; ++i)
            neuralNetInput[i] = 0.0f;

        // neural nets
        int[] layerSizes = new int[] { numInputs, 20, 20, numOutputs };
        neuralNet = new NeuralNetwork(layerSizes);
    }

    private void Update()
    {
        UpdateFeelers();
    }

    void UpdateFeelers()
    {
        var layerMaskObstacle = LayerMask.GetMask("Obstacle");
        var layerMaskFood = LayerMask.GetMask("Food");

        var angleMin = -angleSpreadDegrees;
        var angleDelta = (angleSpreadDegrees * 2.0f) / (numFeelers - 1.0f);

        var rayOrigin = transform.position;

        var resultHitFood = Physics2D.CircleCast(rayOrigin, feelerDist, transform.forward, feelerDist, layerMaskFood);

        var dirToFood = Vector3.zero;
        var distToFood = 0.0f;

        if (resultHitFood)
        {
            var collisionPoint = resultHitFood.collider.transform.
                position;

            dirToFood = collisionPoint - rayOrigin;
            distToFood = dirToFood.magnitude;

            Debug.DrawLine(rayOrigin, collisionPoint, Color.yellow);
        }

        for (int iFeeler = 0; iFeeler < numFeelers; ++iFeeler)
        {
            var angle = angleMin + angleDelta * iFeeler;
            var lineDir = Quaternion.Euler(0.0f, 0.0f, angle) * transform.up;
            var rayDirection = lineDir;

            var resultHitObstacle = Physics2D.Raycast(rayOrigin, rayDirection, feelerDist, layerMaskObstacle);

            if (resultHitObstacle)
            {
                // contact!
                Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * resultHitObstacle.distance, Color.red);

                feelerDanger[iFeeler] = 1.0f - resultHitObstacle.distance / feelerDist;
            }
            else
            {
                // no contact!
                Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * feelerDist, Color.blue);

                feelerDanger[iFeeler] = 0.0f;
            }

            // hit food
            if (resultHitFood)
            {
                var angleBetween = Vector3.Angle(dirToFood, rayDirection);

                var angleMax = angleDelta;
                var angleWeight = 1.0f- Mathf.Clamp01(angleBetween / angleMax);
                feelerHunger[iFeeler] = Mathf.Clamp01((1.0f- distToFood/feelerDist) * angleWeight);
            }
            else
            {
                feelerHunger[iFeeler] = 0.0f;
            }
        }
    }

    void UpdateNeuralNetOutput()
    {
        for(int i = 0; i < numFeelers; ++i)
        {
            neuralNetInput[i] = feelerDanger[i];
            neuralNetInput[i + numFeelers] = feelerHunger[i];
        }

        var fwd = transform.up;

        neuralNetInput[numFeelers * 2+0] = rb.velocity.magnitude;
        neuralNetInput[numFeelers * 2+1] = rb.angularVelocity;
        neuralNetInput[numFeelers * 2+2] = Mathf.Atan2(fwd.y, fwd.x);

        // feed forward
        neuralNet.FeedForward(neuralNetInput, neuralNetOutput);
    }

    CreatureThruster CreateThruster(Transform parent)
    {
        var thrusterObj = GameObject.Instantiate(thrusterProto);
        thrusterObj.transform.SetParent(parent, false);
        thrusterObj.transform.localPosition = Vector3.zero;
        thrusterObj.transform.localRotation = Quaternion.identity;

        CreatureThruster thruster = thrusterObj.GetComponent<CreatureThruster>();
        return thruster;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            OnDeath();
        }

        else if (collision.gameObject.layer == LayerMask.NameToLayer("Food"))
        {
            print("EAT FOOD");
            //OnDeath();
        }
    }

    IEnumerator HandleSteering()
    {
        while(true)
        {
            UpdateNeuralNetOutput();

            Vector2 fwd = transform.up;
            Vector2 left = fwd;
            Vector2 right = fwd;

            var forceScalar = 0.1f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) )
            {
                rb.AddForceAtPosition(left * forceScalar, thrusterLeft.transform.position);
                thrusterLeft.EmitParticles(1);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                rb.AddForceAtPosition(right * forceScalar, thrusterRight.transform.position);
                thrusterRight.EmitParticles(1);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.transform.position = Vector3.zero;
                rb.transform.rotation = Quaternion.identity;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0.0f;
            }

            var forceLeft = neuralNetOutput[0];
            var forceRight = neuralNetOutput[1];

            if (forceLeft > 0.0f)
            {
                rb.AddForceAtPosition(left * forceScalar * forceLeft, thrusterLeft.transform.position);
                thrusterLeft.EmitParticles(1);
            }

            if (forceRight > 0.0f)
            {
                rb.AddForceAtPosition(right * forceScalar * forceRight, thrusterRight.transform.position);
                thrusterRight.EmitParticles(1);
            }

            UpdateNeuralNetOutput();


            yield return null;
        }
    }
}
