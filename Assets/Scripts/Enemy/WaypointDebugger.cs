#if UNITY_EDITOR
#pragma warning disable 649

using UnityEngine;

[ExecuteInEditMode]
public class WaypointDebugger : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;
    [Space(5)]
    [SerializeField] private float debugWaypointSize = 0.5f;
    [SerializeField] private Color debugWaypointColour = Color.red;
    [SerializeField] private Color debugWaypointLineColour = Color.green;

    private void Update()
    {
        var waypoints = enemyController.waypoints;
        var waypointCount = waypoints.Length;

        if (waypointCount > 0)
        {
            if (waypointCount > 1)
            {
                for (var i = 0; i < waypointCount - 1; i++)
                {
                    Debug.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position, debugWaypointLineColour);
                    this.DebugDrawWaypoint(waypoints[i].transform.position, debugWaypointSize, debugWaypointColour);
                }

                if (enemyController.looping)
                {
                    Debug.DrawLine(waypoints[0].transform.position, waypoints[waypointCount - 1].transform.position, debugWaypointLineColour);
                }
            }
            this.DebugDrawWaypoint(waypoints[waypointCount - 1].transform.position, debugWaypointSize, debugWaypointColour);
        }
    }

    private void DebugDrawWaypoint(Vector3 position, float size, Color colour)
    {
        var halfSize = size * 0.5f;

        var min = new Vector3(position.x - halfSize, position.y, position.z);
        var max = new Vector3(position.x + halfSize, position.y, position.z);
        Debug.DrawLine(min, max, colour);

        min = new Vector3(position.x, position.y - halfSize, position.z);
        max = new Vector3(position.x, position.y + halfSize, position.z);
        Debug.DrawLine(min, max, colour);

        min = new Vector3(position.x, position.y, position.z - halfSize);
        max = new Vector3(position.x, position.y, position.z + halfSize);
        Debug.DrawLine(min, max, colour);
    }

    //private void DebugDrawWaypoint(Vector3 position, float size, Color colour)
    //{
    //    var centre = transform.position;
    //    var yRotation = transform.eulerAngles.y;
    //    var halfSize = size * 0.5f;

    //    var min = centre + this.RotatePointAroundAxis(new Vector3(-halfSize, 0f, 0f), yRotation, Vector3.up);
    //    var max = centre + this.RotatePointAroundAxis(new Vector3(halfSize, 0f, 0f), yRotation, Vector3.up);
    //    Debug.DrawLine(min, max, colour);

    //    min = new Vector3(centre.x, centre.y - halfSize, centre.z);
    //    max = new Vector3(centre.x, centre.y + halfSize, centre.z);
    //    Debug.DrawLine(min, max, colour);

    //    max = centre + this.RotatePointAroundAxis(new Vector3(0f, 0f, halfSize), yRotation, Vector3.up);
    //    Debug.DrawLine(centre, max, colour);
    //}

    //private Vector3 RotatePointAroundAxis(Vector3 point, float angle, Vector3 axis)
    //{
    //    return Quaternion.AngleAxis(angle, axis) * point;
    //}
}

#pragma warning restore 649
#endif
