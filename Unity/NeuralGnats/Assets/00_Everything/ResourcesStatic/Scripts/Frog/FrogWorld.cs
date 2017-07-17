using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    public GridPos gridPos = GridPos.Zero;
    public GameObject entity = null;
}

public class FrogWorld : SingletonMonoBehaviourOnDemand<FrogWorld>
{
    public static float gridSize = 0.5f;
    public const int numGridsX = 20;
    public const int numGridsZ = 100;

    List<GridData> world = new List<GridData>();

    public GameObject floor;
    public GameObject protoTree;

	void OnEnable()
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

        CreateObstacles();
    }

    void CreateObstacles()
    {
        var perlinScale = 25.0f / Mathf.Max(numGridsX, numGridsZ);

        float aspect = (float)numGridsZ / (float)numGridsX;

        for (int z = 0; z < numGridsZ; ++z) 
        {
            for (int x = 0; x < numGridsX; ++x)
            {
                var gridData = new GridData();
                gridData.gridPos = new GridPos(x, z);
                world.Add(gridData);

                var isWall = z == 0 || x == 0 || x == (numGridsX - 1);

                if (!isWall && z < 4)
                    continue;

                if (isWall || Mathf.PerlinNoise((float)x * aspect * perlinScale, (float)z * perlinScale) < 0.3f)
                {
                    var tree = GameObject.Instantiate(protoTree);
                    tree.transform.localScale = Vector3.one * gridSize;
                    tree.transform.position = GridToWorldPos(gridData.gridPos);

                    gridData.entity = tree;
                }
            }
        }

        protoTree.SetActive(false);
    }

    public bool HasObstacle(GridPos gridPos)
    {
        return GetGridData(gridPos).entity != null;
    }

    public GridData GetGridData(GridPos gridPos)
    {
        int index = gridPos.z * numGridsX + gridPos.x;
        return world[index];
    }
}
