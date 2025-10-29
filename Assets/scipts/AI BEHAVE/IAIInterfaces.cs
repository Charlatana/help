using System;
using UnityEngine;

public interface IHealth
{
    float Current { get; }
    float Max { get; }
    bool IsDead { get; }
    float Percent { get; } // Current / Max
    event Action<float> OnHealthChanged; // sends new percent (0..1)
}

public interface IMovement
{
    /// <summary>Move to world position (pathing is naive: direct movement).</summary>
    void MoveTo(Vector3 worldPos);

    /// <summary>Stop movement immediately.</summary>
    void Stop();

    /// <summary>Reverse/back away with given speed (meters/sec).</summary>
    void Reverse(float speed);

    /// <summary>Rotate the body to face world position (instant or smooth depending on implementation).</summary>
    void RotateTowards(Vector3 worldPos);
}

public interface ITargeting
{
    Transform CurrentTarget { get; }
    Vector3? LastKnownPosition { get; }
    bool IsTargetVisible(Transform t);
    event Action<Transform> OnTargetAcquired;
    event Action<Transform> OnTargetLost;

    /// <summary>Face the provided world position with turret/gun (optional).</summary>
    void FacePosition(Vector3 worldPos);

    /// <summary>Fire using your ammo system. Adapter will wire to existing VisualShotSystem/ammoSystem.</summary>
    void FireAt(Vector3 worldPos);
}
