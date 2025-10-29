using UnityEngine;
using System.Collections.Generic;

public class PatrolBehaviour : Behaviour
{
    private float pauseAtWaypoint;
    private float pauseTimer = 0f;
    private Vector3 currentWaypoint;
    private bool hasWaypoint = false;

    private BoxCollider patrolBox;

    public PatrolBehaviour(AIController ai, Blackboard bb, float pauseAtWaypoint = 1f) : base(ai, bb)
    {
        this.pauseAtWaypoint = pauseAtWaypoint;
        // try to find a child named "PatrolArea" or a BoxCollider
        patrolBox = ai.GetComponentInChildren<BoxCollider>();
    }

    public override void Enter()
    {
        // pick first waypoint immediately
        PickNewWaypoint();

    }


    public override void Tick()
    {
        if (ai.movement == null) return;

        if (!hasWaypoint)
        {
            PickNewWaypoint();
            return;
        }

        float dist = Vector3.Distance(ai.transform.position, currentWaypoint);
        if (dist > 1.2f)
        {
            ai.movement.MoveTo(currentWaypoint);
        }
        else
        {
            // reached, pause then choose another
            ai.movement.Stop();
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseAtWaypoint)
            {
                pauseTimer = 0f;
                hasWaypoint = false;
            }
        }
        ai.movement.MoveTo(currentWaypoint);
        Debug.Log("Patrolling...");
    }

    public override void Exit()
    {
        ai.movement?.Stop();
    }

    void PickNewWaypoint()
    {
        if (patrolBox != null)
        {
            Vector3 center = patrolBox.transform.TransformPoint(patrolBox.center);
            Vector3 ext = patrolBox.size * 0.5f;
            Vector3 worldExt = Vector3.Scale(ext, patrolBox.transform.lossyScale);

            float rx = Random.Range(-worldExt.x, worldExt.x);
            float rz = Random.Range(-worldExt.z, worldExt.z);
            currentWaypoint = center + new Vector3(rx, 0f, rz);
            hasWaypoint = true;
        }
        else
        {
            // fallback: random near current position
            currentWaypoint = ai.transform.position + (Random.insideUnitSphere * 6f);
            currentWaypoint.y = ai.transform.position.y;
            hasWaypoint = true;
        }
    }
}
