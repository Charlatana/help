using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FleeBehaviour : Behaviour
{
    private float fleeDistance = 12f;
    private float fleeSpeed = 6f;
    private Vector3 fleeTarget;
    private bool fleeing = false;

    public FleeBehaviour(AIController ai, Blackboard bb) : base(ai, bb) { }

    public override void Enter()
    {
        fleeing = false;
        ai.movement?.Stop();
    }

    public override void Tick()
    {
        if (ai.movement == null) return;

        // if we have a recent target, run away from it; otherwise reverse straight
        Vector3 source = ai.transform.position;
        Vector3 dir;
        if (blackboard.CurrentTarget != null)
        {
            dir = (source - blackboard.CurrentTarget.position).normalized;
        }
        else if (blackboard.HasRecentTarget(3f))
        {
            dir = (source - blackboard.LastKnownTargetPos).normalized;
        }
        else
        {
            // reverse along current backward vector
            dir = -ai.transform.forward;
        }

        fleeTarget = ai.transform.position + dir * fleeDistance;
        ai.movement.MoveTo(fleeTarget);
        fleeing = true;

        // disable firing: Ask controller or targeting adapter to not fire while fleeing
        // we do this by clearing blackboard.CurrentTarget locally:
        // the AIController decision logic will not set Attack while Flee is active (priority)
    }

    public override void Exit()
    {
        fleeing = false;
        ai.movement?.Stop();
    }
}
