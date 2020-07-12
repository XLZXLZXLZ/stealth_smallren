using System;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Waypoint
{
    public Transform transform;
    public float waitDuration;

    [HideInInspector] public Vector3 position
    {
        get
        {
            NavMesh.SamplePosition(transform.position, out var hit, 1000f, NavMesh.AllAreas);
            return hit.position;
        }
    }
}
