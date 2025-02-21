/*
 * ZombieAI.cs - script is written by ThunderWire Games
 * Script for all AI Actions
 * Version 1.0
*/

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCHealth))]
public class ZombieAI1 : MonoBehaviour
{
    [HideInInspector]public NPCHealth npcHealth;
    private NavMeshAgent navMeshA;
    private GameObject player;

    [Header("Main Setup")]
    public Animator zombieAnimator;
    public LayerMask SearchMask;
    public LayerMask PlayerMask;
    public PatrolPoint[] patrolPoints;

    [Header("Sensors")]
    public AttackTrigger AttackCollider;
    public WaypointGroup Waypoints;

    [Header("Sensor Settings")]
    [Range(0, 179)]
    public float zombieFOVAngle;
    public float closeRadius;
    public float seeDistance;

    [Header("AI Settings")]
    [Range(0, 1)]
    public int inteligence;
    public float walkSpeed;
    public float runSpeed;
    public float patrolPointDetect;
    public int patrolTime;
    public int patrolPointTime;
    public bool randomPatrol;
    public bool waypointPatrol;

    [Header("AI Damage Settings")]
    [Tooltip("Attack Damage given to Player (From-To)")]
    public Vector2 AttackDamage;

    private bool playerStealth;

    private int patrolRandomTime;

    int baseLayerIdle = Animator.StringToHash("Base Layer.Idle");
    int baseLayerWalk = Animator.StringToHash("Base Layer.Walking");
    int baseLayerRun = Animator.StringToHash("Base Layer.Running");

    int screamHash = Animator.StringToHash("Scream");
    int kickHash = Animator.StringToHash("Kick");
    int attackHash = Animator.StringToHash("Attack");

    private Vector3 lasSeenPos;
    private Vector3 patrolPointPos;

    private float playerDistance;

    public int path;
    public bool enableGizmos;

    private int oldWaypoint;

    [HideInInspector] public bool isAttracted;
    private bool playerChased;
    private bool increase;
    private bool patrol;
    private bool patrolPending;
    private bool goLastPos;
    private bool isDead = false;

    private void Awake()
    {
        navMeshA = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
        patrolPoints = FindObjectsOfType<PatrolPoint>();

        if (GetComponent<NPCHealth>())
        {
            npcHealth = GetComponent<NPCHealth>();
        }
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = zombieAnimator.GetCurrentAnimatorStateInfo(0);

        if (!npcHealth) return;

        if (player && !isDead)
        {
            playerDistance = Vector3.Distance(transform.position, player.transform.position);
            playerStealth = player.GetComponent<PlayerController>().state == 1;
            if (AttackCollider.PlayerInTrigger)
            {
                StopAllCoroutines();
                patrol = false;
                navMeshA.isStopped = true;
                SetAnimatorState("isAttacking", true, true);
                
            }
            else
            {
                if (!stateInfo.IsName("Attack"))
                {
                    navMeshA.isStopped = false;

                    if (inteligence != 0 && npcHealth.damageTaken && !isAttracted)
                    {
                        isAttracted = true;
                    }

                    if (SearchForPlayer())
                    {
                        StopAllCoroutines();
                        playerChased = true;
                        if(inteligence != 0){ isAttracted = true; }
                        patrol = false;
                        SetAnimatorState("isRunning", true, true);
                        lasSeenPos = player.transform.position;
                        navMeshA.SetDestination(lasSeenPos);
                        navMeshA.speed = runSpeed;
                        patrolPointPos = Vector3.zero;
                        goLastPos = false;
                    }
                    else
                    {
                        if (playerChased)
                        {
                            if (!goLastPos)
                            {
                                navMeshA.SetDestination(lasSeenPos);
                                goLastPos = true;
                            }

                            if (patrolPoints.Length > 0)
                            {
                                foreach (var i in patrolPoints)
                                {
                                    if (i.InTrigger)
                                    {
                                        patrolPointPos = i.transform.position;
                                    }
                                }
                            }

                            if (pathComplete())
                            {
                                if (inteligence != 0)
                                {
                                    if (patrolPointPos != Vector3.zero)
                                    {
                                        float distance = Vector3.Distance(lasSeenPos, patrolPointPos);
                                        Debug.Log("Current distance: " + distance + " Detect at: " + patrolPointDetect);
                                        if (distance <= patrolPointDetect)
                                        {
                                            Debug.Log("Setting Destination to Patrol Point");
                                            SetAnimatorState("isWalking", true, true);
                                            navMeshA.speed = walkSpeed;
                                            navMeshA.SetDestination(patrolPointPos);
                                            StartCoroutine(GoToPatrolPoint());

                                            foreach (var i in patrolPoints)
                                            {
                                                if (i.zombieInTrigger)
                                                {
                                                    i.zombieInTrigger = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    StartCoroutine(Patrol(patrolTime));
                                    SetAnimatorState("Patrol", true, true);
                                }
                                playerChased = false;
                                isAttracted = false;
                                npcHealth.damageTaken = false;
                            }
                        }
                        else
                        {
                            if (!patrol)
                            {
                                WaypointSequence();
                            }
                        }
                    }
                }
                else
                {
                    SetAnimatorState("isRunning", true, true);
                }
            }
        }
    }

    void SetAnimatorState(string parameter, bool state, bool disableOthers)
    {
        if (disableOthers)
        {
            zombieAnimator.SetBool("Patrol", false);
            zombieAnimator.SetBool("isRunning", false);
            zombieAnimator.SetBool("isWalking", false);
            zombieAnimator.SetBool("isAttacking", false);
        }

        if (state)
        {
            zombieAnimator.SetBool(parameter, true);
        }else{
            zombieAnimator.SetBool(parameter, false);
        }
    }

    void WaypointSequence()
    {
        if (pathComplete())
        {
            if (waypointPatrol)
            {
                if (!patrolPending)
                {
                    SetAnimatorState("isWalking", true, true);
                    navMeshA.speed = walkSpeed;
                    navMeshA.SetDestination(Waypoints.waypoints[path].position);
                    increase = false;
                }
                else
                {
                    SetAnimatorState("Patrol", true, true);
                    patrolRandomTime = Random.Range(1, patrolTime);
                    StartCoroutine(Patrol(patrolRandomTime));
                }
                patrolPending = true;
            }
            else
            {
                SetAnimatorState("isWalking", true, true);
                navMeshA.speed = walkSpeed;
                navMeshA.SetDestination(Waypoints.waypoints[path].position);
                increase = false;
            }

            NextWaypoint();
        }
    }

    private void NextWaypoint()
    {
        if (randomPatrol)
        {
            if (Waypoints.waypoints.Count > 1)
            {
                System.Random rnd = new System.Random();
                path = rnd.Next(0, Waypoints.waypoints.Count);
            }
            else
            {
                path = path == Waypoints.waypoints.Count - 1 ? 0 : path + 1;
            }
        }
        else
        {
            if (path < Waypoints.waypoints.Count && !increase)
            {
                path++;
                if (path > (Waypoints.waypoints.Count - 1)) path = 0;
                increase = true;
            }
        }
    }

    IEnumerator Patrol(int Time)
    {
        patrol = true;
        yield return new WaitForSeconds(Time);
        patrol = false;
        patrolPending = false;
    }

    IEnumerator GoToPatrolPoint()
    {
        patrol = true;
        yield return new WaitUntil(() => pathComplete());
        SetAnimatorState("Patrol", true, true);
        yield return new WaitForSeconds(patrolPointTime);
        patrol = false;
        patrolPointPos = Vector3.zero;
    }

    public void AttackPlayer()
    {
        float randomDamage = Random.Range(AttackDamage.x, AttackDamage.y);
       
        if (AttackCollider.PlayerInTrigger)
            player.GetComponent<HealthManager>().ApplyDamage(randomDamage);
        Debug.Log("Damage");
    }

    public void StateMachine(bool enabled)
    {
        if (!enabled)
        {
            isDead = true;
            zombieAnimator.enabled = false;
            navMeshA.isStopped = true;
        }
        else
        {
            StopAllCoroutines();
            isDead = false;
            zombieAnimator.enabled = true;
            navMeshA.isStopped = false;
        }
    }

    private bool pathComplete()
    {
        if (Vector3.Distance(navMeshA.destination, navMeshA.transform.position) <= navMeshA.stoppingDistance)
        {
            if (!navMeshA.hasPath || navMeshA.velocity.sqrMagnitude <= 0.05f)
            {
                return true;
            }
        }
        return false;
    }

    private bool SearchForPlayer()
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, player.transform.position, out hit, SearchMask))
        {
            if (hit.collider.gameObject == player && playerDistance <= seeDistance)
            {
                if (!isAttracted)
                {
                    if (IsLookingAtPlayer())
                    {
                        return true;
                    }
                    else
                    {
                        if (IsPlayerClose())
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsLookingAtPlayer()
    {
        float checkAngle = Mathf.Min(zombieFOVAngle, 359.9999f) / 2;

        float dot = Vector3.Dot(transform.forward, (player.transform.position - transform.position).normalized);

        float viewAngle = (1 - dot) * 90;

        if (viewAngle <= checkAngle)
            return true;
        else
            return false;
    }

    private bool IsPlayerClose()
    {
        if (Physics.CheckSphere(transform.position + GetComponent<CapsuleCollider>().center, closeRadius, PlayerMask))
        {
            if (!playerStealth)
                return true;
            else
                return false;
        }
        else
        {
            return false;
        }
    }

    public List<string> GetZombieData()
    {
        return new List<string>()
        {
            isDead.ToString(),
            transform.localPosition.x.ToString(),
            transform.localPosition.y.ToString(),
            transform.localPosition.z.ToString(),
            npcHealth.Health.ToString(),
            isAttracted.ToString(),
            path.ToString(),
            patrolPending.ToString(),
            patrol.ToString(),
            patrolPointPos.x.ToString(),
            patrolPointPos.y.ToString(),
            patrolPointPos.z.ToString(),
            lasSeenPos.x.ToString(),
            lasSeenPos.y.ToString(),
            lasSeenPos.z.ToString(),
            transform.localEulerAngles.y.ToString()
        };
    }

    public void GoZombiePatrol()
    {
        StopAllCoroutines();
        navMeshA.ResetPath();
        patrolPending = true;
        SetAnimatorState("isWalking", true, true);
        navMeshA.speed = walkSpeed;
        navMeshA.SetDestination(Waypoints.waypoints[path].position);
        increase = false;
    }

    public void GoZombiePatrolPoint(Vector3 patrolPoint, Vector3 lastPos)
    {
        StopAllCoroutines();
        navMeshA.ResetPath();
        patrolPointPos = patrolPoint;
        lasSeenPos = lastPos;
        goLastPos = true;
        playerChased = true;
    }

    void OnDrawGizmosSelected()
    {
        float rayRange = 10.0f;
        float halfFOV = zombieFOVAngle / 2.0f;

        if (!enableGizmos) return;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + GetComponent<CapsuleCollider>().center, closeRadius);
    }
}
