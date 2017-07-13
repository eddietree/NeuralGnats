using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour {

    public GameObject thrusterProto;

    public Transform thrusterLeftMount;
    public Transform thrusterRightMount;
    CreatureThruster thrusterLeft;
    CreatureThruster thrusterRight;
    Rigidbody2D rb;

    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();

        thrusterLeft = CreateThruster(thrusterLeftMount);
        thrusterRight = CreateThruster(thrusterRightMount);
        thrusterProto.SetActive(false);

        StartCoroutine(HandleSteering());	
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

    IEnumerator HandleSteering()
    {
        while(true)
        {
            Vector2 fwd = transform.up;


            //Vector2 left = transform.position - thrusterLeft.transform.position;
            //Vector2 left = transform.position - thrusterLeft.transform.position;
            //Vector2 left = -transform.right;
            //Vector2 right = transform.right;
            Vector2 left = fwd;
            Vector2 right = fwd;


            //rb.AddForce(fwd*0.1f);

            var forceScalar = 0.1f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
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

            yield return null;
        }
    }
}
