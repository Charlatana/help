using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;

public class AttackBehaviour : Behaviour
{
    private float fireInterval = 0.1f; // throttle
    private float lastFireTime = -999f;
    private float engagementDistance = 30f;

    public AttackBehaviour(AIController ai, Blackboard bb) : base(ai, bb) { }

    public override void Enter()
    {
        // stop moving; we'll reposition only if target lost
        ai.movement?.Stop();
    }

    public override void Tick()
    {
        if (ai.targeting == null || ai.movement == null) return;
        var targetTransform = blackboard.CurrentTarget ?? null;

        if (targetTransform == null)
        {
            // no live target, controller will switch to standby/patrol after timeout
            return;
        }

        // If target visible, stop and fire. If not visible but we have last known pos, move there.
        if (ai.targeting.IsTargetVisible(targetTransform))
        {
            // face target
            ai.targeting.FacePosition(targetTransform.position);
            ai.movement?.Stop();

            // Rate-limited firing (also allow adapters to check reload)
            if (Time.time - lastFireTime >= fireInterval)
            {
                ai.targeting.FireAt(targetTransform.position);
                lastFireTime = Time.time;
            }
        }
        else
        {
            // move to last known position so we can reacquire
            if (blackboard.HasRecentTarget(5f))
            {
                ai.movement.MoveTo(blackboard.LastKnownTargetPos);
            }
        }
    }

    public override void Exit()
    {
        ai.movement?.Stop();
    }
}
