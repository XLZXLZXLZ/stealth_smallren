using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelElevatorTarget : MonoBehaviour
{
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(5f, .1f, 5f));
    }
}
