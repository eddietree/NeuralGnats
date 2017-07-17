using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSimulation : SimulationBase
{
    override public void OnUpdateSimulation()
    {
        base.OnUpdateSimulation();

        var camera = Camera.main;
        var creatures = SimulationManager.Instance.creatures;

        var minX = 9999.0f;
        var minZ = 9999.0f;
        var maxX = -9999.0f;
        var maxZ = -9999.0f;

        foreach (var creature in creatures)
        {
            if (creature.isDead)
                continue;

            var pos = creature.transform.position;

            minX = Mathf.Min(pos.x, minX);
            minZ = Mathf.Min(pos.z, minZ);
            maxX = Mathf.Max(pos.x, maxX);
            maxZ = Mathf.Max(pos.z, maxZ);

            var centerX = (minX + maxX) * 0.5f;
            var centerZ = (minZ + maxZ) * 0.5f;
            var dimenX = maxX - minX;
            var dimenY = maxZ - minZ;

            var camPos = camera.transform.position;
            var camPosNew = new Vector3(centerX, camPos.y, centerZ);

            camera.transform.position = Vector3.Lerp(camPos, camPosNew, 0.001f);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, Mathf.Max(dimenX, dimenY) + 4.0f, 0.001f);
        }
    }
}
