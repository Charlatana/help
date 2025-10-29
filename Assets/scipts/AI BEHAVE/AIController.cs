using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;
using UnityEditor;

[RequireComponent(typeof(Transform))]
public class AIController : MonoBehaviour
{
    [Header("Adapters (auto-find if null)")]
    public MonoBehaviour targetingComponent; // set to component with TargetingAdapter
    public MonoBehaviour movementComponent;  // set to component with MovementAdapter
    public MonoBehaviour healthComponent;    // set to component with HealthAdapter

    [Header("Behaviour settings")]
    public float lostTargetTimeout = 5f;
    public float fleeHealthThreshold = 0.5f; // 50%

    // exposed for debug/tuning
    public float standbyDuration = 4f;
    public float patrolPauseAtWaypoint = 1f;

    [Header("Runtime (read only)")]
    public Behaviour activeBehaviour;
    public Behaviour patrolBehaviour;
    public Behaviour attackBehaviour;
    public Behaviour fleeBehaviour;
    public Behaviour standbyBehaviour;

    [HideInInspector] public ITargeting targeting;
    [HideInInspector] public IMovement movement;
    [HideInInspector] public IHealth health;
    [HideInInspector] public Blackboard blackboard;

    void Awake()
    {
        blackboard = new Blackboard();
    }

    void Start()
    {
        // try auto-find adapters if null
        if (targetingComponent == null) targetingComponent = GetComponentInChildren<TargetingAdapter>() as MonoBehaviour;
        if (movementComponent == null) movementComponent = GetComponent<MovementAdapter>() as MonoBehaviour;
        if (healthComponent == null) healthComponent = GetComponent<HealthAdapter>() as MonoBehaviour;

        targeting = targetingComponent as ITargeting;
        movement = movementComponent as IMovement;
        health = healthComponent as IHealth;

        if (targeting == null) Debug.LogWarning($"{name} AIController: Targeting adapter not found.");
        if (movement == null) Debug.LogWarning($"{name} AIController: Movement adapter not found.");
        if (health == null) Debug.LogWarning($"{name} AIController: Health adapter not found.");

        // create behaviours
        patrolBehaviour = new PatrolBehaviour(this, blackboard, patrolPauseAtWaypoint);
        attackBehaviour = new AttackBehaviour(this, blackboard);
        fleeBehaviour = new FleeBehaviour(this, blackboard);
        standbyBehaviour = new StandbyBehaviour(this, blackboard, standbyDuration);

        // initial behaviour
        SetBehaviour(patrolBehaviour);

        // hook events
        if (targeting != null)
        {
            targeting.OnTargetAcquired += OnTargetAcquired;
            targeting.OnTargetLost += OnTargetLost;
        }

        if (health != null)
        {
            health.OnHealthChanged += OnHealthChanged;
            // initialize blackboard health
            blackboard.HealthPercent = health.Percent;
        }
    }

    void Update()
    {
        // Decision priority: Flee > Attack > Standby > Patrol
        if (health != null && health.Percent <= fleeHealthThreshold)
        {
            if (!(activeBehaviour is FleeBehaviour))
                SetBehaviour(fleeBehaviour);
        }
        else if (targeting != null && targeting.CurrentTarget != null && targeting.IsTargetVisible(targeting.CurrentTarget))
        {
            if (!(activeBehaviour is AttackBehaviour))
                SetBehaviour(attackBehaviour);

            // update blackboard
            blackboard.CurrentTarget = targeting.CurrentTarget;
            blackboard.LastKnownTargetPos = targeting.CurrentTarget.position;
            blackboard.LastSeenTime = Time.time;
        }
        else
        {
            // lost or no target: if we recently saw one -> Standby (look), else Patrol
            if (blackboard.HasRecentTarget(lostTargetTimeout))
            {
                if (!(activeBehaviour is StandbyBehaviour))
                    SetBehaviour(standbyBehaviour);
            }
            else
            {
                if (!(activeBehaviour is PatrolBehaviour))
                    SetBehaviour(patrolBehaviour);
            }
        }

        // tick active behaviour
        activeBehaviour?.Tick();

    }


    void SetBehaviour(Behaviour b)
    {
        if (activeBehaviour != null) activeBehaviour.Exit();
        activeBehaviour = b;
        activeBehaviour?.Enter();
    }

    // events
    void OnTargetAcquired(Transform t)
    {
        blackboard.CurrentTarget = t;
        blackboard.LastKnownTargetPos = t.position;
        blackboard.LastSeenTime = Time.time;
    }

    void OnTargetLost(Transform t)
    {
        if (blackboard.CurrentTarget == t)
        {
            // keep lastKnown and lastSeenTime already set by OnTargetAcquired
            blackboard.CurrentTarget = null;
        }
    }

    void OnHealthChanged(float percent)
    {
        blackboard.HealthPercent = percent;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AIController))]
    public class AIControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            AIController ai = (AIController)target;

            GUILayout.Space(10);
            GUILayout.Label("🔧 Behaviour Debug Panel", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Patrol")) ai.DebugSetBehaviour(ai.patrolBehaviour);
            if (GUILayout.Button("Attack")) ai.DebugSetBehaviour(ai.attackBehaviour);
            if (GUILayout.Button("Flee")) ai.DebugSetBehaviour(ai.fleeBehaviour);
            if (GUILayout.Button("Standby")) ai.DebugSetBehaviour(ai.standbyBehaviour);
            GUILayout.EndHorizontal();

            if (ai.activeBehaviour != null)
            {
                GUILayout.Space(5);
                GUILayout.Label($"🧠 Active: {ai.activeBehaviour.GetType().Name}", EditorStyles.helpBox);
            }
        }
    }
#endif

    public void DebugSetBehaviour(Behaviour b)
    {
        SetBehaviour(b);
        Debug.Log($"[AI] {name} manually switched to {b.GetType().Name}");
    }
}
