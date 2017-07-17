using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    public GridPos gridPos = GridPos.Zero;
    public GameObject entity = null;
}

public class FrogWorld : MonoBehaviour
{
    public static float gridSize = 0.5f;
    int numGridsX = 20;
    int numGridsZ = 100;

    List<GridData> grid = new List<GridData>();

    public GameObject floor;
    public GameObject protoTree;

	void Start ()
    {
        CreateWorld();
	}
	
	void Update ()
    {
		
	}

    public static Vector3 GridToWorldPos(GridPos gridPos)
    {
        var result = new Vector3(gridPos.x * gridSize, gridSize*0.5f, gridPos.z * gridSize);
        return result;
    }

    void CreateWorld()
    {
        float dimenX = numGridsX * gridSize;
        float dimenZ = numGridsZ * gridSize;

        // position floor
        floor.transform.position = new Vector3(dimenX * 0.5f - gridSize*0.5f, 0.0f, dimenZ * 0.5f - gridSize * 0.5f);
        floor.transform.localScale = new Vector3(dimenX * 0.5f * 0.2f, 1.0f, dimenZ * 0.5f * 0.2f);
        floor.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", new Vector2(0.5f*numGridsX, 0.5f*numGridsZ));

        CreateTrees();
    }

    void CreateTrees()
    {
        var perlinScale = 30.0f / Mathf.Max(numGridsX, numGridsZ);

        float aspect = (float)numGridsZ / (float)numGridsX;

        for (int x = 0; x < numGridsX; ++x)
        {
            for (int z =4; z < numGridsZ; ++z)
            {
                if (Mathf.PerlinNoise((float)x * aspect * perlinScale, (float)z * perlinScale) < 0.3f)
                {
                    GridPos gridPos = new GridPos(x, z);

                    var tree = GameObject.Instantiate(protoTree);
                    tree.transform.localScale = Vector3.one * gridSize;
                    tree.transform.position = GridToWorldPos(gridPos);
                }
            }
        }

        protoTree.SetActive(false);
    }
}
