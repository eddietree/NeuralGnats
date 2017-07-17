﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    public GameObject entity = null;
}

public class FrogWorld : SingletonMonoBehaviourOnDemand<FrogWorld>
{
    public static float gridSize = 0.5f;
    public const int numGridsX = 20;
    public const int numGridsZ = 250;

    List<GridData> world = new List<GridData>();

    public GameObject floor;
    public GameObject protoTree;
    public TextMesh textMeshMarker;

	void OnEnable()
    {
        CreateWorld();
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
        var perlinScale = 50.0f / Mathf.Max(numGridsX, numGridsZ);
        var perlinThreshold = 0.35f;

        float aspect = (float)numGridsZ / (float)numGridsX;

        for (int z = 0; z < numGridsZ; ++z) 
        {
            for (int x = 0; x < numGridsX; ++x)
            {
                var gridPos = new GridPos(x, z);

                var gridData = new GridData();
                world.Add(gridData);

                var isWall = z == 0 || x == 0 || x == (numGridsX - 1);

                if (!isWall && z < 4)
                    continue;

                if (isWall || Mathf.PerlinNoise((float)x * aspect * perlinScale, (float)z * perlinScale) < perlinThreshold)
                {
                    var tree = GameObject.Instantiate(protoTree);
                    tree.transform.localScale = Vector3.one * gridSize;
                    tree.transform.position = GridToWorldPos(gridPos);

                    gridData.entity = tree;
                }
            }
        }

        protoTree.SetActive(false);

        // markers
        for (int z = 5; z < numGridsZ; z+=5)
        {
            // left side
            var gridPos0 = new GridPos(-2, z);
            var textPos0 = GridToWorldPos(gridPos0);
            textPos0.y -= gridSize * 0.5f;

            var textObj0 = GameObject.Instantiate(textMeshMarker.gameObject);
            textObj0.transform.position = textPos0;
            textObj0.GetComponent<TextMesh>().text = string.Format("{0} -", z);

            // right side
            var gridPos1 = new GridPos(numGridsX, z);
            var textPos1 = GridToWorldPos(gridPos1);
            textPos1.y -= gridSize * 0.5f;

            var textObj1 = GameObject.Instantiate(textMeshMarker.gameObject);
            textObj1.transform.position = textPos1;
            textObj1.GetComponent<TextMesh>().text = string.Format("- {0}", z);
            textObj1.GetComponent<TextMesh>().anchor = TextAnchor.MiddleLeft;
            textObj1.GetComponent<TextMesh>().alignment = TextAlignment.Left;
        }
        textMeshMarker.gameObject.SetActive(false);
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

    public bool IsValidGridPos(GridPos gridPos)
    {
        return gridPos.x >= 0 
            && gridPos.x < numGridsX
            && gridPos.z >= 0
            && gridPos.z < numGridsZ;
    }
}
