using UnityEngine;
using System;

[RequireComponent(typeof(AITankTargeting))]
public class TargetingAdapter : MonoBehaviour, ITargeting
{
    public AITankTargeting inner;
    private Transform lastTarget;

    public event Action<Transform> OnTargetAcquired;
    public event Action<Transform> OnTargetLost;

    public Transform CurrentTarget => inner != null ? GetInnerTarget() : null;
    public Vector3? LastKnownPosition { get; private set; }

    void Awake() { inner = GetComponent<AITankTargeting>(); }

    void Start()
    {
        // no direct events in original; we use polling + trigger notifications the original script already calls
        // If original AITankTargeting had events, we'd hook them. We'll poll in Update here.
    }

    void Update()
    {
        var cur = GetInnerTarget();
        if (cur != lastTarget)
        {
            if (cur != null)
            {
                LastKnownPosition = cur.position;
                OnTargetAcquired?.Invoke(cur);
            }
            else if (lastTarget != null)
            {
                OnTargetLost?.Invoke(lastTarget);
            }
            lastTarget = cur;
        }

        // update last known pos
        if (cur != null) LastKnownPosition = cur.position;
    }

    Transform GetInnerTarget()
    {
        // original stores private "target" field; can't access directly.
        // We'll try to read via reflection or provide a small public accessor on your AITankTargeting later.
        // For now attempt reflection lookup:
        var tField = typeof(AITankTargeting).GetField("target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (tField != null)
        {
            return tField.GetValue(inner) as Transform;
        }
        return null;
    }

    public bool IsTargetVisible(Transform t)
    {
        if (t == null) return false;
        // Simple line of sight: raycast from firePoint to target, if no obstacles then visible
        var firePointField = typeof(AITankTargeting).GetField("firePoint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Transform fp = firePointField != null ? firePointField.GetValue(inner) as Transform : null;
        Vector3 origin = fp != null ? fp.position : inner.transform.position + Vector3.up * 1.2f;
        Vector3 dir = (t.position - origin).normalized;
        if (Physics.Raycast(origin, dir, out RaycastHit h, 200f))
        {
            return h.transform == t;
        }
        return false;
    }

    public void FacePosition(Vector3 worldPos)
    {
        // call inner turret rotation: rotate turret transform
        if (inner == null) return;
        var turret = inner.turret;
        if (turret == null) return;

        Vector3 dir = new Vector3(worldPos.x - turret.position.x, 0f, worldPos.z - turret.position.z);
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion desired = Quaternion.LookRotation(dir);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, desired, inner.engageTurnSpeed * Time.deltaTime);
    }

    public void FireAt(Vector3 worldPos)
    {
        // call the existing VisualShotSystem via the inner script info
        if (inner == null) return;
        var firePoint = inner.firePoint;
        if (firePoint == null) firePoint = inner.transform;

        // the original script used ammoSystem - get it with reflection
        var ammoField = typeof(AITankTargeting).GetField("ammoSystem", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var ammoSystem = ammoField != null ? ammoField.GetValue(inner) as TankAmmoSystem : null;

        VisualShotSystem.Instance?.Fire(firePoint.position, worldPos, ammoSystem);
    }
}
