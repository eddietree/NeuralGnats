using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : CreatureBase
{
    public GameObject thrusterProto;

    public Transform thrusterLeftMount;
    public Transform thrusterRightMount;
    CreatureThruster thrusterLeft;
    CreatureThruster thrusterRight;
    public Rigidbody2D rb;

    const int numFeelers = 7;
    const float angleSpreadDegrees = 90.0f;
    const float feelerDist = 5.5f;

    float[] feelerDanger = new float[numFeelers];
    float[] feelerHunger = new float[numFeelers];

    public delegate void DeathEvent();
    public delegate void EatFoodEvent(GameObject food);
    public DeathEvent eventDeath;
    public EatFoodEvent eventEatFood;

    float lifeSpanEat = 5.0f;
    float lifeSpanMax = 10.0f;

    HashSet<GameObject> touchedZones = new HashSet<GameObject>();

    Coroutine threadSteering;

    bool hungerEnabled = false;

    void Start ()
    {
        thrusterLeft = CreateThruster(thrusterLeftMount);
        thrusterRight = CreateThruster(thrusterRightMount);
        thrusterProto.SetActive(false);

        Reset();
	}

    private void Reset()
    {
        isDead = false;
        lifeSpan = lifeSpanMax;
        fitness = 0.0f;

        if (threadSteering != null)
        {
            StopCoroutine(threadSteering);
            threadSteering = null;
        }

        threadSteering = StartCoroutine(HandleSteering());
    }

    void OnDeath()
    {
        if (eventDeath != null)
            eventDeath();

        eventDeath = null;
        eventEatFood = null;

        if (threadSteering != null)
        {
            StopCoroutine(threadSteering);
            threadSteering = null;
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0.0f;

        thrusterLeft.gameObject.SetActive(false);
        thrusterRight.gameObject.SetActive(false);

        isDead = true;
        GetComponent<BoxCollider2D>().enabled = false;

        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    public override void InitNeuralNetwork()
    {
        // feelers, velocity, angular velocity
        int numInputs = feelerDanger.Length + 2;
        if (hungerEnabled)
            numInputs += feelerHunger.Length;

        int numOutputs = 2;

        neuralNetInput = new float[numInputs];
        neuralNetOutput = new float[numOutputs];

        for (int i = 0; i < neuralNetInput.Length; ++i)
            neuralNetInput[i] = 0.0f;

        for (int i = 0; i < neuralNetOutput.Length; ++i)
            neuralNetOutput[i] = 0.0f;

        // neural nets
        int[] layerSizes = new int[] { numInputs, 7, numOutputs };
        neuralNet = new NeuralNetwork(layerSizes);
    }

    void UpdateFeelers()
    {
        //var layerMaskObstacle = LayerMask.GetMask("Obstacle", "Creature");
        var layerMaskObstacle = LayerMask.GetMask("Obstacle");
        var layerMaskFood = LayerMask.GetMask("Food");

        var angleMin = -angleSpreadDegrees;
        var angleDelta = (angleSpreadDegrees * 2.0f) / (numFeelers - 1.0f);

        var rayOrigin = transform.position + transform.up * 0.1f;

        var resultHitFood = Physics2D.CircleCast(rayOrigin, feelerDist, transform.forward, feelerDist, layerMaskFood);

        var dirToFood = Vector3.zero;
        var distToFood = 0.0f;

        if (resultHitFood)
        {
            var collisionPoint = resultHitFood.collider.transform.
                position;

            dirToFood = collisionPoint - rayOrigin;
            distToFood = dirToFood.magnitude;

            if ( SimulationManager.showDebugLines)
                Debug.DrawLine(rayOrigin, collisionPoint, Color.yellow);
        }

        for (int iFeeler = 0; iFeeler < numFeelers; ++iFeeler)
        {
            var angle = angleMin + angleDelta * iFeeler;
            var lineDir = Quaternion.Euler(0.0f, 0.0f, angle) * transform.up;
            var rayDirection = lineDir;

            var resultHitObstacle = Physics2D.Raycast(rayOrigin, rayDirection, feelerDist, layerMaskObstacle);

            if (resultHitObstacle) // contact!
            {
                var feelDangerVal = Mathf.Clamp01(1.0f - resultHitObstacle.distance / feelerDist);
                feelerDanger[iFeeler] = feelDangerVal;

                if (SimulationManager.showDebugLines)
                {
                    var lineColor = Color.Lerp(Color.black, Color.red, feelDangerVal + 0.1f);

                    lineColor.a = feelDangerVal;

                    Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * resultHitObstacle.distance, lineColor);
                }
            }
            else // no contact!
            {
                if (SimulationManager.showDebugLines)
                    Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * feelerDist, Color.blue);

                feelerDanger[iFeeler] = 0.0f;
            }

            // hit food
            if (resultHitFood)
            {
                var angleBetween = Vector3.Angle(dirToFood, rayDirection);

                var angleMax = angleDelta;
                var angleWeight = 1.0f- Mathf.Clamp01(angleBetween / angleMax);
                var hungerVal = Mathf.Clamp01((1.0f - distToFood / feelerDist) * angleWeight);

                feelerHunger[iFeeler] = hungerVal;
            }
            else
            {
                feelerHunger[iFeeler] = 0.0f;
            }
        }
    }

    void UpdateNeuralNetOutput()
    {
        int index = 0;

        for(int i = 0; i < feelerDanger.Length; ++i)
        {
            neuralNetInput[index++] = feelerDanger[i];
        }

        if (hungerEnabled)
        {
            for (int i = 0; i < feelerHunger.Length; ++i)
            {
                neuralNetInput[index++] = feelerHunger[i];
            }
        }

        neuralNetInput[index++] = rb.velocity.magnitude;
        neuralNetInput[index++] = rb.angularVelocity;

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

    void OnTriggerEnter2D(Collider2D collisionObj)
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
            OnDeath();
        }

        else if (collisionObj.gameObject.layer == LayerMask.NameToLayer("Food"))
        {
            if (eventEatFood != null)
                eventEatFood(collisionObj.gameObject);

            //fitness += 0.05f;
            lifeSpan = Mathf.Min(lifeSpan+lifeSpanEat, lifeSpanMax);

            //collisionObj.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            //GameObject.Destroy(collisionObj.gameObject);
        }
    }

    IEnumerator HandleSteering()
    {
        while (true)
        {
            UpdateFeelers();
            UpdateNeuralNetOutput();

            Vector2 fwd = transform.up;
            Vector2 left = fwd;
            Vector2 right = fwd;

            var forceScalar = 0.15f;

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

                if (Random.Range(0.0f,1.0f) < forceLeft)
                    thrusterLeft.EmitParticles(1);
            }

            if (forceRight > 0.0f)
            {
                rb.AddForceAtPosition(right * forceScalar * forceRight, thrusterRight.transform.position);

                if (Random.Range(0.0f, 1.0f) < forceRight)
                    thrusterRight.EmitParticles(1);
            }

            lifeSpan -= Time.deltaTime;
            if (lifeSpan < 0.0f)
            {
                OnDeath();
            }

            //fitness += Time.deltaTime * 0.05f;

            yield return null;
        }
    }
}
