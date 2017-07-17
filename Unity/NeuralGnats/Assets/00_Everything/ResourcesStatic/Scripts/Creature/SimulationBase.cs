using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationBase : MonoBehaviour
{
    virtual public void OnStartSimulation()
    {
    }

    virtual public void OnStopSimulation()
    {
    }

    virtual public void OnUpdateSimulation()
    {
    }

    virtual public void OnCreatureCreated(CreatureBase creature)
    {
    }
}
