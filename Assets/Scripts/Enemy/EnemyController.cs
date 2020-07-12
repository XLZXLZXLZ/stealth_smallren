#pragma warning disable 649

using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Waypoint[] waypoints;
    public bool looping = true;
    [Space(5)]
    [SerializeField] private float coneOfVisionAngle = 135f;
    [SerializeField] private float coneOfVisionDistance = 4f;
    [SerializeField] private float maxInstinctualDistance = 2f;
    [SerializeField] private float patrollingSpeed = 1f;
    [SerializeField] private float chasingSpeed = 2f;
    [SerializeField] private float lostPlayerWaitTime = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    private CapsuleCollider playerCapsuleCollider;
    private Transform playerTransform;
    private EnemyState state = EnemyState.Patrolling;
    private int targetWaypointIndex;
    private bool waypointsReversed;
    private float currentWaypointWaitTime;

    // Animations
    private Vector3 lastPosition;
    private float currentVelocity;
    private Animator animator;

    // Patrolling
    private NavMeshPath patrollingPath;
    private int patrollingPathCornerIndex;
    private Vector3 patrollingPathPosition;

    // Chasing
    private NavMeshPath pathToPlayerLastSeen;
    private int pathToPlayerLastSeenCornerIndex;
    private Vector3 pathToPlayerLastSeenPosition;

    // Waiting
    private float timeSincePlayerLastSeenPositionReached;

    // Returning
    private NavMeshPath returnToPatrollingPath;
    private Vector3 returnToPatrollingPathPosition;

    private void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints");
            return;
        }

        patrollingPathPosition = waypoints[targetWaypointIndex].position;
        this.transform.position = patrollingPathPosition;

        this.NewTargetWaypoint();

        patrollingPath = new NavMeshPath();
        NavMesh.CalculatePath(patrollingPathPosition, waypoints[targetWaypointIndex].position, NavMesh.AllAreas, patrollingPath);

        pathToPlayerLastSeen = new NavMeshPath();
        returnToPatrollingPath = new NavMeshPath();

        animator = gameObject.GetComponentInChildren<Animator>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!TankCharacterController.Instance || waypoints.Length == 0)
        {
            return;
        }
        if (!playerCapsuleCollider)
        {
            playerCapsuleCollider = TankCharacterController.Instance.gameObject.GetComponentInChildren<CapsuleCollider>();
        }
        if (!playerTransform)
        {
            playerTransform = TankCharacterController.Instance.gameObject.transform;
        }

        // Setting enemy state to Chasing if player can be seen
        if (this.CanSeePlayer())
        {
            NavMesh.CalculatePath(this.transform.position, this.GetPlayerNavPosition(), NavMesh.AllAreas, pathToPlayerLastSeen);
            pathToPlayerLastSeenCornerIndex = 1;
            state = EnemyState.Chasing;
        }

        // Setting the patrolling path position

        var target = patrollingPath.corners[patrollingPathCornerIndex];

        patrollingPathPosition = Vector3.MoveTowards(patrollingPathPosition, target, patrollingSpeed * Time.deltaTime);

        if (patrollingPathPosition == waypoints[targetWaypointIndex].position)
        {
            currentWaypointWaitTime += Time.deltaTime;

            if (currentWaypointWaitTime >= waypoints[targetWaypointIndex].waitDuration)
            {
                currentWaypointWaitTime = 0f;
                patrollingPathCornerIndex = 0;
                this.NewTargetWaypoint();
                NavMesh.CalculatePath(patrollingPathPosition, waypoints[targetWaypointIndex].position, NavMesh.AllAreas, patrollingPath);
            }
        }
        else if (patrollingPathPosition == target)
        {
            patrollingPathCornerIndex++;
        }

        // Setting the player last seen path position

        if (state == EnemyState.Chasing)
        {
            target = pathToPlayerLastSeen.corners[pathToPlayerLastSeenCornerIndex];

            pathToPlayerLastSeenPosition = Vector3.MoveTowards(this.transform.position, target, chasingSpeed * Time.deltaTime);

            if (pathToPlayerLastSeenPosition == target)
            {
                if (pathToPlayerLastSeenCornerIndex + 1 == pathToPlayerLastSeen.corners.Length)
                {
                    state = EnemyState.Searching;
                }
                else
                {
                    pathToPlayerLastSeenCornerIndex++;
                }
            }
        }

        // Enemy is searching

        if (state == EnemyState.Searching)
        {
            timeSincePlayerLastSeenPositionReached += Time.deltaTime;
            if (timeSincePlayerLastSeenPositionReached >= lostPlayerWaitTime)
            {
                timeSincePlayerLastSeenPositionReached = 0f;
                state = EnemyState.Returning;
            }
        }

        // Setting the return to patrolling path position

        if (state == EnemyState.Returning)
        {
            NavMesh.CalculatePath(this.transform.position, patrollingPathPosition, NavMesh.AllAreas, returnToPatrollingPath);

            target = returnToPatrollingPath.corners[1];

            returnToPatrollingPathPosition = Vector3.MoveTowards(this.transform.position, target, patrollingSpeed * Time.deltaTime);

            if (returnToPatrollingPathPosition == patrollingPathPosition)
            {
                state = EnemyState.Patrolling;
            }
        }

        // Setting the position
        if (state == EnemyState.Patrolling)
        {
            this.transform.position = patrollingPathPosition;

            foreach (var corner in patrollingPath.corners)
            {
                this.DebugDrawCorner(corner, 0.5f, Color.cyan);
            }
        }
        else if (state == EnemyState.Chasing)
        {
            this.transform.position = pathToPlayerLastSeenPosition;

            foreach (var corner in pathToPlayerLastSeen.corners)
            {
                this.DebugDrawCorner(corner, 0.5f, Color.cyan);
            }
        }
        else if (state == EnemyState.Returning)
        {
            this.transform.position = returnToPatrollingPathPosition;

            foreach (var corner in returnToPatrollingPath.corners)
            {
                this.DebugDrawCorner(corner, 0.5f, Color.cyan);
            }
        }

        // Setting the rotation
        if (state != EnemyState.Searching)
        {
            var lookDirection = (target - this.transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(lookDirection, this.transform.up);
                var degreeDiff = Quaternion.Angle(this.transform.rotation, targetRotation);
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * degreeDiff);
            }
        }

        var distanceToPlayer = Vector3.Distance(TankCharacterController.Instance.transform.position, transform.position);
        if (distanceToPlayer <= 1.2f)
        {
            TankCharacterController.Instance.Kill();
        }

        this.UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        var distance = Vector3.Distance(this.transform.position, lastPosition);
        lastPosition = this.transform.position;
        var velocity = distance / Time.deltaTime;

        currentVelocity += (velocity - currentVelocity) * Time.deltaTime * 2f;
        if (currentVelocity <= patrollingSpeed)
        {
            var speed = Mathf.Lerp(0f, .5f, Mathf.Clamp(currentVelocity / patrollingSpeed, 0f, 1f));
            animator.SetFloat("Speed", speed);
        }
        else
        {
            var speed = Mathf.Lerp(.5f, 1f, Mathf.Clamp((currentVelocity + patrollingSpeed) / (patrollingSpeed - patrollingSpeed), 0f, 1f));
            animator.SetFloat("Speed", speed);
        }
    }

    private Vector3 GetPlayerNavPosition()
    {
        NavMesh.SamplePosition(playerTransform.position, out var hit, 1000f, NavMesh.AllAreas);
        return hit.position;
    }

    private void NewTargetWaypoint()
    {
        if (waypoints.Length <= 1)
        {
            return;
        }

        if (waypoints.Length == 2)
        {
            targetWaypointIndex = targetWaypointIndex == 0 ? 1 : 0;
        }
        else
        {
            if (looping)
            {
                targetWaypointIndex++;
                if (targetWaypointIndex == waypoints.Length)
                {
                    targetWaypointIndex = 0;
                }
            }
            else
            {
                if (waypointsReversed)
                {
                    targetWaypointIndex--;
                    if (targetWaypointIndex == -1)
                    {
                        waypointsReversed = false;
                        targetWaypointIndex = 1;
                    }
                }
                else
                {
                    targetWaypointIndex++;
                    if (targetWaypointIndex == waypoints.Length)
                    {
                        waypointsReversed = true;
                        targetWaypointIndex = waypoints.Length - 2;
                    }
                }
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (Vector3.Distance(this.transform.position, this.GetPlayerNavPosition()) <= maxInstinctualDistance)
        {
            return true;
        }

        if (!this.IsPlayerWithinDistance() || !this.IsPlayerWithinRadius())
        {
            return false;
        }

        if (!TankCharacterController.Instance.Alive)
        {
            return false;
        }

        var halfAngle = coneOfVisionAngle / 2;

        var testLeft = true;
        var testRight = true;

        var toPlayerCentre = (playerTransform.position - this.transform.position).normalized;

        var angleToPlayerCentre = Vector3.SignedAngle(this.transform.forward, toPlayerCentre, Vector3.up);
        if (angleToPlayerCentre < -halfAngle)
        {
            toPlayerCentre = Quaternion.Euler(0f, -halfAngle, 0f) * this.transform.forward;
            testLeft = false;
        }
        else if (angleToPlayerCentre > halfAngle)
        {
            toPlayerCentre = Quaternion.Euler(0f, halfAngle, 0f) * this.transform.forward;
            testRight = false;
        }

        if (Physics.Raycast(this.transform.position, toPlayerCentre, out var hit, coneOfVisionDistance) && hit.collider == playerCapsuleCollider)
        {
            return true;
        }

        if (!testLeft && !testRight)
        {
            return false;
        }

        this.GetEnemyToPlayerSides(out var toPlayerLeft, out var toPlayerRight);

        if (testLeft)
        {
            var angleToPlayerLeft = Vector3.SignedAngle(this.transform.forward, toPlayerLeft, Vector3.up);
            if (angleToPlayerLeft < -halfAngle)
            {
                toPlayerLeft = Quaternion.Euler(0f, -halfAngle, 0f) * this.transform.forward;
            }

            if (Physics.Raycast(this.transform.position, toPlayerLeft, out hit, coneOfVisionDistance) && hit.collider == playerCapsuleCollider)
            {
                return true;
            }
        }

        if (testRight)
        {
            var angleToPlayerRight = Vector3.SignedAngle(this.transform.forward, toPlayerRight, Vector3.up);
            if (angleToPlayerRight > halfAngle)
            {
                toPlayerRight = Quaternion.Euler(0f, halfAngle, 0f) * this.transform.forward;
            }

            if (Physics.Raycast(this.transform.position, toPlayerRight, out hit, coneOfVisionDistance) && hit.collider == playerCapsuleCollider)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPlayerWithinDistance()
    {
        var toPlayer = (playerTransform.position - this.transform.position).normalized;
        var closestPlayerPoint = playerTransform.position - (toPlayer * playerCapsuleCollider.radius);

        return Vector3.Distance(this.transform.position, closestPlayerPoint) <= coneOfVisionDistance;
    }

    private bool IsPlayerWithinRadius()
    {
        this.GetEnemyToPlayerSides(out var toPlayerLeft, out var toPlayerRight);
        var angleToPlayerLeft = Vector3.Angle(this.transform.forward, toPlayerLeft);
        var angleToPlayerRight = Vector3.Angle(this.transform.forward, toPlayerRight);

        var halfAngle = coneOfVisionAngle / 2;

        return angleToPlayerLeft <= halfAngle || angleToPlayerRight <= halfAngle;
    }

    private void GetEnemyToPlayerSides(out Vector3 toPlayerLeft, out Vector3 toPlayerRight)
    {
        var toPlayer = (playerTransform.position - this.transform.position).normalized;

        var perpendicular = Quaternion.Euler(0f, 90f, 0f) * toPlayer * playerCapsuleCollider.radius;

        var playerLeft = playerTransform.position - perpendicular;
        var playerRight = playerTransform.position + perpendicular;

        toPlayerLeft = (playerLeft - this.transform.position).normalized;
        toPlayerRight = (playerRight - this.transform.position).normalized;
    }

    private void DebugDrawCorner(Vector3 position, float size, Color colour)
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
}

#pragma warning restore 649
