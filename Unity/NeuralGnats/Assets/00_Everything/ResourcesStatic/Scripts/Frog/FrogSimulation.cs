using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSimulation : SimulationBase
{
    Vector3 camDelta = new Vector3(10.0f, 10.0f, -10.0f);

    public static float simTimeScale = 1.0f;

    private void OnGUI()
    {
        var buttonWidth = 130;
        var buttonHeight = 25;
        var buttonMargin = 6;

        var posX = Screen.width - buttonWidth - buttonMargin;
        var posY = buttonMargin;

        // Toggle Speed
        if (GUI.Button(new Rect(posX, posY, buttonWidth, buttonHeight), "Toggle Speed"))
            simTimeScale = simTimeScale == 1.0f ? 0.05f : 1.0f;

        posY += buttonMargin + buttonHeight;

        // reset simulation
        if (GUI.Button(new Rect(posX, posY, buttonWidth, buttonHeight), "Reset Simulation"))
            SimulationManager.Instance.RestartSimulation();
    }

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
