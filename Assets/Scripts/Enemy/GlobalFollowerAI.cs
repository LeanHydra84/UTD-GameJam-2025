using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class GlobalFollowerAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    
    [Header("Behavior")]
    public float followDistance = 3f;
    [Range(5f, 45f)]
    public float visionConeAngle = 15f;
    public float freezeTime = 3f;
    public float escapeTime = 20f;
    public float resumeFollowDelay = 2f; // delay before resuming follow after freeze
    
    [Header("Movement")]
    public float normalSpeed = 3.5f;
    public float catchUpSpeed = 6f; // faster speed when far from player
    public float catchUpDistance = 30f; // distance at which AI speeds up
    public float escapeSpeed = 8f; // speed when running away
    
    [Header("Global Tracking")]
    public bool enableGlobalTracking = true; // always know player position
    public float maxFollowDistance = 1000f; // maximum distance before giving up
    public float pathUpdateInterval = 0.5f; // how often to recalculate path when far away

    [SerializeField] private float killDistance = 1;
        
        
    // Internal state
    private NavMeshAgent agent;
    private Camera playerCam;
    private int state = 0; // 0=following, 1=frozen, 2=escaping, 3=resuming
    private float timer;
    private float nextPathUpdate;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 escapeTarget;

    void OnDisable()
    {
        if (agent == null) return;
        agent.enabled = false;
    }

    void OnEnable()
    {
        if (agent == null) return;
        agent.enabled = true;
    }
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        
        // find player and camera
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player != null)
        {
            playerCam = player.GetComponentInChildren<Camera>() ?? Camera.main;
            lastKnownPlayerPosition = player.position;
        }
        
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.stoppingDistance = 0.5f;
        
        // start in following state
        SetState(0);
    }
    
    void Update()
    {
        if (player == null) return;
        
        // update last known position if global tracking is enabled
        if (enableGlobalTracking)
        {
            lastKnownPlayerPosition = player.position;
        }
        
        // update timer
        timer -= Time.deltaTime;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        bool inVision = CheckVisionCone();

        state = 0;
        inVision = false;

        if (Vector3.Distance(player.transform.position, transform.position) < killDistance)
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        // state transitions
        switch (state)
        {
            case 0: // following
                HandleFollowing(distToPlayer, inVision);
                break;
                
            case 1: // frozen                if (timer <= 0)
                    SetState(2); // Start escaping
                break;
                
            case 2: // escaping
                HandleEscaping();
                if (timer <= 0)
                    SetState(3); // Start resume delay
                break;
                
            case 3: // resume delay
                if (timer <= 0)
                    SetState(0); // Resume following
                break;
        }
    }
    
    void HandleEscaping()
    {
        if (escapeTarget == Vector3.zero || Vector3.Distance(transform.position, escapeTarget) < 2f)
        {
            // find escape point
            Vector3 awayFromPlayer = (transform.position - player.position).normalized;
            Vector3 targetPosition = transform.position + awayFromPlayer * 20f;
            
            // so random xd
            Vector3 randomOffset = new Vector3(
                Random.Range(-10f, 10f),
                0,
                Random.Range(-10f, 10f)
            );
            targetPosition += randomOffset;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 15f, NavMesh.AllAreas))
            {
                escapeTarget = hit.position;
            }
            else
            {
                // if can't escape, just move away from player
                escapeTarget = transform.position + awayFromPlayer * 10f;
            }
        }
        
        if (escapeTarget != Vector3.zero)
            agent.SetDestination(escapeTarget);
    }
    
    void HandleFollowing(float distToPlayer, bool inVision)
    {
        // if spotted by player stop
        if (inVision)
        {
            SetState(1);
            return;
        }
        
        // player distance check
        if (distToPlayer > maxFollowDistance)
        {
            agent.ResetPath();
            return;
        }
        
        // movement speed based on distance
        if (distToPlayer > catchUpDistance)
        {
            agent.speed = catchUpSpeed;
        }
        else
        {
            agent.speed = normalSpeed;
        }
        
        // update path
        bool shouldUpdatePath = false;
        
        if (distToPlayer <= followDistance)
        {
            // Close enough - stop moving
            agent.ResetPath();
        }
        else if (distToPlayer > catchUpDistance)
        {
            // far
            if (Time.time >= nextPathUpdate)
            {
                shouldUpdatePath = true;
                nextPathUpdate = Time.time + pathUpdateInterval;
            }
        }
        else
        {
            // med
            if (Time.time >= nextPathUpdate)
            {
                shouldUpdatePath = true;
                nextPathUpdate = Time.time + 0.1f;
            }
        }
        
        if (shouldUpdatePath)
        {
            UpdatePathToPlayer();
        }
    }
    
    void UpdatePathToPlayer()
    {
        Vector3 targetPosition = enableGlobalTracking ? lastKnownPlayerPosition : player.position;
        
        // sample a valid position on the NavMesh near the player
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // if can't find valid position near player, try moving closer
            Vector3 directionToPlayer = (targetPosition - transform.position).normalized;
            Vector3 intermediateTarget = transform.position + directionToPlayer * 10f;
            
            if (NavMesh.SamplePosition(intermediateTarget, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
    
    bool CheckVisionCone()
    {
        if (!playerCam) return false;
        
        Vector3 toEnemy = transform.position - playerCam.transform.position;
        toEnemy.y = 0; // Ignore height differences
        
        if (toEnemy.magnitude < 0.1f) return true; // Very close to camera
        
        Vector3 playerForward = playerCam.transform.forward;
        playerForward.y = 0;
        
        if (playerForward.magnitude < 0.1f) return false; // Invalid forward vector
        
        float angle = Vector3.Angle(playerForward.normalized, toEnemy.normalized);
        return angle <= visionConeAngle;
    }
    
    void SetState(int newState)
    {
        state = newState;
        
        switch (state)
        {
            case 0: // following
                agent.speed = normalSpeed;
                break;
                
            case 1: // frozen
                timer = freezeTime;
                agent.speed = 0;
                agent.ResetPath();
                break;
                
            case 2: // escaping
                timer = escapeTime;
                agent.speed = escapeSpeed;
                escapeTarget = Vector3.zero; // Force recalculation of escape target
                break;
                
            case 3: // resume delay
                timer = resumeFollowDelay;
                agent.speed = 0;
                agent.ResetPath();
                break;
        }
    }
    
    // teleport AI closer if it gets too far behind
    public void TeleportToPlayer(float maxDistance = 50f)
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > maxDistance)
        {
            Vector3 teleportPos = player.position + Random.onUnitSphere * 10f;
            teleportPos.y = player.position.y; // Keep same height
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(teleportPos, out hit, 15f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
    }
    
    // debug
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // draw follow distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, followDistance);
        
        // draw catch up distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, catchUpDistance);
        
        // draw max follow distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxFollowDistance);
        
        // draw line to player
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, player.position);
    }
}