#pragma warning disable 649

using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Waypoint[] waypoints;  // 敌人可以巡逻的路径点数组
    public bool looping = true;  // 是否循环巡逻
    [Space(5)]
    [SerializeField] private float coneOfVisionAngle = 135f;  // 视野锥角度
    [SerializeField] private float coneOfVisionDistance = 4f;  // 视野距离
    [SerializeField] private float maxInstinctualDistance = 2f;  // 最大本能追逐距离
    [SerializeField] private float patrollingSpeed = 1f;  // 巡逻速度
    [SerializeField] private float chasingSpeed = 2f;  // 追逐速度
    [SerializeField] private float lostPlayerWaitTime = 2f;  // 失去目标玩家后等待时间
    [SerializeField] private float rotationSpeed = 10f;  // 转向速度
    private CapsuleCollider playerCapsuleCollider;  // 玩家的胶囊碰撞器
    private Transform playerTransform;  // 玩家的变换
    private EnemyState state = EnemyState.Patrolling;  // 敌人的状态，默认为巡逻状态
    private Light searchLight;
    private Color lightColor;
    private int targetWaypointIndex;  // 目标路径点索引
    private bool waypointsReversed;  // 路径点是否反向
    private float currentWaypointWaitTime;  // 当前路径点等待时间

    // 动画
    private Vector3 lastPosition;  // 上一个位置
    private float currentAnimatorSpeed;  // 当前动画速度
    private Animator animator;  // 动画控制器

    // 巡逻
    private NavMeshPath patrollingPath;  // 巡逻路径
    private int patrollingPathCornerIndex;  // 巡逻路径拐角索引
    private Vector3 patrollingPathPosition;  // 巡逻路径位置

    // 追逐
    private NavMeshPath pathToPlayerLastSeen;  // 到最后一次看到玩家的路径
    private int pathToPlayerLastSeenCornerIndex;  // 到最后一次看到玩家的路径拐角索引
    private Vector3 pathToPlayerLastSeenPosition;  // 到最后一次看到玩家的路径位置

    // 等待
    private float timeSincePlayerLastSeenPositionReached;  // 自最后一次看到玩家位置以来的时间

    // 返回
    private NavMeshPath returnToPatrollingPath;  // 返回巡逻路径
    private Vector3 returnToPatrollingPathPosition;  // 返回巡逻路径位置

    public EnemyState EnemyState
        => State;

    private EnemyState State
    {
        get
        {
            return state;
        }

        set
        {
            if(state != value)
            {
                OnStateChange(state, value);
                state = value;
            }
        }
    }

    private void OnStateChange(EnemyState oldState, EnemyState newState)
    {
        if(newState == EnemyState.Chasing)
            searchLight.color = Color.red;
        else if(oldState == EnemyState.Chasing)
            searchLight.color = Color.white;
    }

    private void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints");  // 如果没有指定路径点，则在控制台输出警告信息
            return;
        }

        patrollingPathPosition = waypoints[targetWaypointIndex].position;  // 设置初始巡逻路径点为第一个路径点
        this.transform.position = patrollingPathPosition;  // 将敌人位置设置为初始巡逻路径点位置

        this.NewTargetWaypoint();  // 寻找下一个目标路径点

        patrollingPath = new NavMeshPath();  // 初始化巡逻路径
        NavMesh.CalculatePath(patrollingPathPosition, waypoints[targetWaypointIndex].position, NavMesh.AllAreas, patrollingPath);  // 计算到目标路径点的巡逻路径

        pathToPlayerLastSeen = new NavMeshPath();  // 初始化到最后一次看到玩家的路径
        returnToPatrollingPath = new NavMeshPath();  // 初始化返回巡逻路径

        animator = gameObject.GetComponentInChildren<Animator>();  // 获取动画控制器
        searchLight = gameObject.GetComponentInChildren<Light>();
        lastPosition = transform.position;  // 设置上一个位置为当前位置
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

        // 如果能看到玩家，则设置敌人状态为追逐
        if (this.CanSeePlayer())
        {
            NavMesh.CalculatePath(this.transform.position, this.GetPlayerNavPosition(), NavMesh.AllAreas, pathToPlayerLastSeen);
            pathToPlayerLastSeenCornerIndex = 1;
            State = EnemyState.Chasing;
        }

        // 设置巡逻路径点位置
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

        // 设置最后一次看到玩家的路径点位置
        if (State == EnemyState.Chasing)
        {
            target = pathToPlayerLastSeen.corners[pathToPlayerLastSeenCornerIndex];
            pathToPlayerLastSeenPosition = Vector3.MoveTowards(this.transform.position, target, chasingSpeed * Time.deltaTime);

            if (pathToPlayerLastSeenPosition == target)
            {
                if (pathToPlayerLastSeenCornerIndex + 1 == pathToPlayerLastSeen.corners.Length)
                {
                    State = EnemyState.Searching;
                }
                else
                {
                    pathToPlayerLastSeenCornerIndex++;
                }
            }
        }

        // 搜索模式
        if (State == EnemyState.Searching)
        {
            timeSincePlayerLastSeenPositionReached += Time.deltaTime;
            if (timeSincePlayerLastSeenPositionReached >= lostPlayerWaitTime)
            {
                timeSincePlayerLastSeenPositionReached = 0f;
                State = EnemyState.Returning;
            }
        }

        // 设置返回到巡逻路径位置
        if (State == EnemyState.Returning)
        {
            NavMesh.CalculatePath(this.transform.position, patrollingPathPosition, NavMesh.AllAreas, returnToPatrollingPath);

            target = returnToPatrollingPath.corners[1];

            returnToPatrollingPathPosition = Vector3.MoveTowards(this.transform.position, target, patrollingSpeed * Time.deltaTime);

            if (returnToPatrollingPathPosition == patrollingPathPosition)
            {
                State = EnemyState.Patrolling;
            }
        }

        // 设置位置
        if (State == EnemyState.Patrolling)
        {
            this.transform.position = patrollingPathPosition;

            foreach (var corner in patrollingPath.corners)
            {
                this.DebugDrawCorner(corner, 0.5f, Color.cyan);
            }
        }
        else if (State == EnemyState.Chasing)
        {
            this.transform.position = pathToPlayerLastSeenPosition;

            foreach (var corner in pathToPlayerLastSeen.corners)
            {
                this.DebugDrawCorner(corner, 0.5f, Color.cyan);
            }
        }
        else if (State == EnemyState.Returning)
        {
            this.transform.position = returnToPatrollingPathPosition;

            foreach (var corner in returnToPatrollingPath.corners)
            {
                this.DebugDrawCorner(corner, 0.5f, Color.cyan);
            }
        }

        // 设置旋转
        if (State != EnemyState.Searching)
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
        // 计算速度并更新动画状态
        var distance = Vector3.Distance(this.transform.position, lastPosition);
        lastPosition = this.transform.position;
        var velocity = distance / Time.deltaTime;
        var animatorSpeed = 0f;

        if (velocity <= patrollingSpeed)
        {
            // 当速度低于巡逻速度时，将animatorSpeed在0到0.5之间进行插值
            animatorSpeed = Mathf.Lerp(0f, .5f, Mathf.Clamp(velocity / patrollingSpeed, 0f, 1f));
        }
        else
        {
            // 当速度高于巡逻速度时，将animatorSpeed在0.5到1之间进行插值
            animatorSpeed = Mathf.Lerp(.5f, 1f, Mathf.Clamp((velocity + patrollingSpeed) / (patrollingSpeed - patrollingSpeed), 0f, 1f));
        }

        // 使用平滑过渡更新animatorSpeed
        currentAnimatorSpeed += (animatorSpeed - currentAnimatorSpeed) * Time.deltaTime * 2f;

        // 设置动画速度参数
        animator.SetFloat("Speed", currentAnimatorSpeed);
    }

    // 获取玩家在导航网格上的位置
    private Vector3 GetPlayerNavPosition()
    {
        NavMesh.SamplePosition(playerTransform.position, out var hit, 1000f, NavMesh.AllAreas);
        return hit.position;
    }

    // 选择新的目标航点
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

    // 检查是否能看到玩家
    private bool CanSeePlayer()
    {
        // 如果与玩家的导航位置距离小于最大直觉距离，则返回true
        if (Vector3.Distance(this.transform.position, this.GetPlayerNavPosition()) <= maxInstinctualDistance)
        {
            return true;
        }

        // 如果玩家不在特定距离内或者不在视野半径内，则返回false
        if (!this.IsPlayerWithinDistance() || !this.IsPlayerWithinRadius())
        {
            return false;
        }

        // 如果坦克角色控制器已经死亡，则返回false
        if (!TankCharacterController.Instance.Alive)
        {
            return false;
        }

        // 计算视野角度的一半
        var halfAngle = coneOfVisionAngle / 2;

        var testLeft = true;
        var testRight = true;

        // 获取到玩家中心的方向向量
        var toPlayerCentre = (playerTransform.position - this.transform.position).normalized;

        // 计算当前朝向与玩家中心方向向量之间的角度
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

        // 检测是否有射线与玩家胶囊碰撞体相交
        if (Physics.Raycast(this.transform.position, toPlayerCentre, out var hit, coneOfVisionDistance) && hit.collider == playerCapsuleCollider)
        {
            return true;
        }

        // 如果左右两侧都不需要测试，则返回false
        if (!testLeft && !testRight)
        {
            return false;
        }

        // 获取敌人到玩家左右侧的方向向量
        this.GetEnemyToPlayerSides(out var toPlayerLeft, out var toPlayerRight);

        // 如果需要测试左侧
        if (testLeft)
        {
            var angleToPlayerLeft = Vector3.SignedAngle(this.transform.forward, toPlayerLeft, Vector3.up);
            if (angleToPlayerLeft < -halfAngle)
            {
                toPlayerLeft = Quaternion.Euler(0f, -halfAngle, 0f) * this.transform.forward;
            }

            // 检测是否有射线与玩家胶囊碰撞体相交
            if (Physics.Raycast(this.transform.position, toPlayerLeft, out hit, coneOfVisionDistance) && hit.collider == playerCapsuleCollider)
            {
                return true;
            }
        }

        // 如果需要测试右侧
        if (testRight)
        {
            var angleToPlayerRight = Vector3.SignedAngle(this.transform.forward, toPlayerRight, Vector3.up);
            if (angleToPlayerRight > halfAngle)
            {
                toPlayerRight = Quaternion.Euler(0f, halfAngle, 0f) * this.transform.forward;
            }

            // 检测是否有射线与玩家胶囊碰撞体相交
            if (Physics.Raycast(this.transform.position, toPlayerRight, out hit, coneOfVisionDistance) && hit.collider == playerCapsuleCollider)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查玩家是否在最大直觉距离内。
    /// </summary>
    private bool IsPlayerWithinDistance()
    {
        // 计算敌人到玩家的方向向量
        var toPlayer = (playerTransform.position - this.transform.position).normalized;
        // 计算最近的玩家点，考虑到玩家的胶囊碰撞体半径
        var closestPlayerPoint = playerTransform.position - (toPlayer * playerCapsuleCollider.radius);

        // 判断敌人和最近的玩家点之间的距离是否小于等于视野范围
        return Vector3.Distance(this.transform.position, closestPlayerPoint) <= coneOfVisionDistance;
    }

    /// <summary>
    /// 检查玩家是否在视野半径内。
    /// </summary>
    private bool IsPlayerWithinRadius()
    {
        // 获取敌人到玩家左右侧的方向向量
        this.GetEnemyToPlayerSides(out var toPlayerLeft, out var toPlayerRight);
        // 计算敌人与玩家左右方向的夹角
        var angleToPlayerLeft = Vector3.Angle(this.transform.forward, toPlayerLeft);
        var angleToPlayerRight = Vector3.Angle(this.transform.forward, toPlayerRight);

        var halfAngle = coneOfVisionAngle / 2;

        // 判断夹角是否在视野角度范围内
        return angleToPlayerLeft <= halfAngle || angleToPlayerRight <= halfAngle;
    }

    /// <summary>
    /// 获取敌人到玩家左右侧的方向向量。
    /// </summary>
    private void GetEnemyToPlayerSides(out Vector3 toPlayerLeft, out Vector3 toPlayerRight)
    {
        // 计算敌人到玩家的方向向量
        var toPlayer = (playerTransform.position - this.transform.position).normalized;
        // 计算敌人到玩家左右侧的垂直向量
        var perpendicular = Quaternion.Euler(0f, 90f, 0f) * toPlayer * playerCapsuleCollider.radius;
        // 计算玩家左右侧的位置
        var playerLeft = playerTransform.position - perpendicular;
        var playerRight = playerTransform.position + perpendicular;
        // 计算敌人到玩家左右侧的方向向量
        toPlayerLeft = (playerLeft - this.transform.position).normalized;
        toPlayerRight = (playerRight - this.transform.position).normalized;
    }

    /// <summary>
    /// 绘制调试用的边框。
    /// </summary>
    private void DebugDrawCorner(Vector3 position, float size, Color colour)
    {
        var halfSize = size * 0.5f;

        // 绘制立方体的边框
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
