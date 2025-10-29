using UnityEngine;

public class StandbyBehaviour : Behaviour
{
    private float duration;
    private float timer = 0f;

    public StandbyBehaviour(AIController ai, Blackboard bb, float duration = 4f) : base(ai, bb)
    {
        this.duration = duration;
    }

    public override void Enter()
    {
        timer = 0f;
        ai.movement?.Stop();
    }

    public override void Tick()
    {
        timer += Time.deltaTime;

        // rotate slowly to "look around"
        if (ai.movement != null)
        {
            // spin around a little
            Vector3 lookPos = ai.transform.position + (ai.transform.forward * 4f);
            ai.movement.RotateTowards(lookPos);
        }

        if (timer >= duration)
        {
            // let controller pick next behaviour (it will switch to Patrol when no recent target)
        }
    }

    public override void Exit()
    {
        // nothing special
    }
}
