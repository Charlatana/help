using UnityEngine;
using System;

public class Blackboard
{
    public Transform CurrentTarget;
    public Vector3 LastKnownTargetPos;
    public float LastSeenTime;
    public float HealthPercent;
    public bool IsUnderFire;
    public Func<bool> CanFireOverride; // optional check

    public bool HasRecentTarget(float withinSeconds)
    {
        return Time.time - LastSeenTime <= withinSeconds;
    }
}
