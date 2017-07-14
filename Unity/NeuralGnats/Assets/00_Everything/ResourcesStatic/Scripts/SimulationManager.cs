using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

    public int generation = 0;
    public List<float> generationFitness = new List<float>();

    public GameObject prefabCreature;
    public GameObject prefabFood;

    void Start ()
    {
        StartCoroutine(DoHandleGenerations());
    }
	
	IEnumerator DoHandleGenerations()
    {
        while(true)
        {
            yield return null;
        }
    }

    void OnGUI()
    {
        var myStyle = new GUIStyle();
        myStyle.fontSize = 24;
        myStyle.normal.textColor = Color.white;

        var strGeneration = string.Format("Generation: {0}", generation);
        GUI.Label(new Rect(10, 10, 100, 20), strGeneration, myStyle);
    }
}
