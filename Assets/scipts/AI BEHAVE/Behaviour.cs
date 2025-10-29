public abstract class Behaviour
{
    protected AIController ai;
    protected Blackboard blackboard;

    public Behaviour(AIController aiController, Blackboard bb)
    {
        ai = aiController;
        blackboard = bb;
    }

    /// <summary>Called once when behaviour becomes active.</summary>
    public virtual void Enter() { }

    /// <summary>Called every frame while active.</summary>
    public abstract void Tick();

    /// <summary>Called when behaviour exits.</summary>
    public virtual void Exit() { }
}
