using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSimulation : SimulationBase
{
    Vector3 camDelta = new Vector3(10.0f, 10.0f, -10.0f);

    public override void OnStartSimulation()
    {
        base.OnStartSimulation();

        var camera = Camera.main;
        camera.transform.position = camDelta;
        camera.transform.LookAt(Vector3.one);
    }

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

            var focusPos = new Vector3(centerX, 0.0f, centerZ);

            var camPos = camera.transform.position;
            var camPosNew = focusPos + camDelta;

            camera.transform.position = Vector3.Lerp(camPos, camPosNew, 0.003f);
            //camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, Mathf.Max(dimenX, dimenY) + 4.0f, 0.001f);
        }
    }
}
