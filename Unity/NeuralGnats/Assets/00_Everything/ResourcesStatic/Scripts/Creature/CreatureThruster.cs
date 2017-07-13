using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureThruster : MonoBehaviour
{
    public ParticleSystem particles;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void EmitParticles(int numParticles)
    {
        particles.Emit(numParticles);
    }
}
