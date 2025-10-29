using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AITankTargeting : MonoBehaviour
{
    public enum AIMode { Watching, Engaging, Engaged }
    public AIMode mode = AIMode.Watching;

    [Header("References")]
    public Transform turret;
    public Transform firePoint;

    [Header("Barrel Settings")]
    public Transform barrel;
    public float maxElevation = 30f;
    public float maxDepression = -10f;

    [Header("Scan Settings")]
    [Range(0f, 360f)] public float scanArcAngle = 120f;
    [Range(0f, 360f)] public float scanCenterOffset = 0f;
    public float scanSpeed = 30f;
    private float scanTimer = 0f;
    private float scanPeriod = 4f;

    [Header("Engagement Settings")]
    public float engageTurnSpeed = 120f;
    public float reloadTime = 3f;
    [Range(0.98f, 0.9999f)] public float aimDotThreshold = 0.995f;

    public TankAmmoSystem ammoSystem;

    [Header("Area of View")]
    public List<Collider> viewColliders = new List<Collider>();

    [Header("Debug Visualization")]
    public bool showScanArc = true;
    public Color scanArcColor = Color.cyan;

    private List<Transform> visibleTargets = new List<Transform>();
    private Transform target;
    private bool turretLocked = false;
    private bool firingLoopRunning = false;
    private Quaternion lockedRotation;
    private Quaternion lastTurretRotation;
    private Quaternion lockedBarrelRotation;
    private float tankForwardAngle;

    private bool isReloading = false;
    private float reloadTimer = 0f;
    private Quaternion initialTurretRotation;

    private void Start()
    {
        if (ammoSystem == null)
            ammoSystem = GetComponent<TankAmmoSystem>();

        foreach (var col in viewColliders)
        {
            if (col != null)
            {
                var triggerHandler = col.gameObject.AddComponent<AITargetTrigger>();
                triggerHandler.Setup(this);
            }
        }

        scanPeriod = (scanArcAngle / scanSpeed) * 2f;
        initialTurretRotation = turret.rotation;
    }

    private void Update()
    {
        tankForwardAngle = transform.eulerAngles.y;

        switch (mode)
        {
            case AIMode.Watching: WatchingMode(); break;
            case AIMode.Engaging: EngagingMode(); break;
            case AIMode.Engaged: EngagedMode(); break;
        }

        if (showScanArc)
            DrawScanArc();
    }

    void WatchingMode()
    {
        scanTimer += Time.deltaTime;
        float oscillation = Mathf.Sin((scanTimer / scanPeriod) * Mathf.PI * 2f);
        float angleOffset = oscillation * (scanArcAngle / 2f);
        float targetAngle = tankForwardAngle + scanCenterOffset + angleOffset;

        Quaternion desiredRotation = Quaternion.Euler(0f, targetAngle, 0f);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, desiredRotation, scanSpeed * 2f * Time.deltaTime);

        Vector3 currentEuler = barrel.localEulerAngles;
        float newX = Mathf.MoveTowardsAngle(currentEuler.x, 0f, engageTurnSpeed * Time.deltaTime);
        barrel.localEulerAngles = new Vector3(newX, currentEuler.y, currentEuler.z);

        AcquireTargetFromVisible();
    }

    void EngagingMode()
    {
        if (TargetInvalid())
        {
            AbortEngagement();
            return;
        }

        Vector3 targetDir = target.position - turret.position;
        Vector3 flatDir = new Vector3(targetDir.x, 0f, targetDir.z).normalized;
        Quaternion desiredTurretRot = Quaternion.LookRotation(flatDir);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, desiredTurretRot, engageTurnSpeed * Time.deltaTime);

        Vector3 aimDir = (target.position - firePoint.position).normalized;
        float pitch = Mathf.Asin(aimDir.y) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, maxDepression, maxElevation);
        Vector3 currentEuler = barrel.localEulerAngles;
        float newPitch = Mathf.MoveTowardsAngle(currentEuler.x, pitch, engageTurnSpeed * Time.deltaTime);
        barrel.localEulerAngles = new Vector3(newPitch, currentEuler.y, currentEuler.z);

        float alignment = Vector3.Dot(turret.forward, flatDir);
        if (alignment >= aimDotThreshold && !firingLoopRunning && !isReloading)
        {
            StartCoroutine(FiringLoop());
        }
    }

    void EngagedMode()
    {
        ResetToWatching();
    }

    private IEnumerator FiringLoop()
    {
        firingLoopRunning = true;

        while (!TargetInvalid())
        {
          
            if (isReloading)
            {
                reloadTimer -= Time.deltaTime;
                if (reloadTimer <= 0f)
                {
                    isReloading = false;
                    reloadTimer = 0f;
                }

                yield return null;
                continue;
            }

          
            Vector3 toTarget = (target.position - firePoint.position).normalized;
            float alignment = Vector3.Dot(turret.forward, toTarget);
            if (alignment < aimDotThreshold * 0.98f)
            {
                yield return null;
                continue;
            }

            Vector3 spreadOffset = Random.insideUnitSphere * 1.5f;
            spreadOffset.z = 0;
            Vector3 shotTarget = target.position + spreadOffset;

            if (Physics.Raycast(firePoint.position, toTarget, out RaycastHit hit, 150f))
            {
                VisualShotSystem.Instance.Fire(firePoint.position, hit.point, ammoSystem);
            }
            else
            {
                Vector3 endPoint = firePoint.position + toTarget * 150f;
                VisualShotSystem.Instance.Fire(firePoint.position, endPoint, ammoSystem);
            }

            isReloading = true;
            reloadTimer = reloadTime;

            yield return null;
        }

        firingLoopRunning = false;
        AbortEngagement();
    }

    void DrawScanArc()
    {
        float radius = 10f;
        int segments = 30;

        float startAngle = tankForwardAngle + scanCenterOffset - (scanArcAngle / 2f);
        float endAngle = tankForwardAngle + scanCenterOffset + (scanArcAngle / 2f);

        Vector3 center = turret.position;
        Vector3 lastPoint = center + Quaternion.Euler(0f, startAngle, 0f) * Vector3.forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
            Vector3 nextPoint = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            Debug.DrawLine(lastPoint, nextPoint, scanArcColor);
            lastPoint = nextPoint;
        }

        Vector3 leftBound = center + Quaternion.Euler(0f, startAngle, 0f) * Vector3.forward * radius;
        Vector3 rightBound = center + Quaternion.Euler(0f, endAngle, 0f) * Vector3.forward * radius;
        Debug.DrawLine(center, leftBound, scanArcColor);
        Debug.DrawLine(center, rightBound, scanArcColor);

        float centerAngle = tankForwardAngle + scanCenterOffset;
        Vector3 centerLine = center + Quaternion.Euler(0f, centerAngle, 0f) * Vector3.forward * radius;
        Debug.DrawLine(center, centerLine, Color.yellow);
    }

    public static class DebugExtension
    {
        public static void DebugCircle(Vector3 position, Vector3 up, Color color, float radius = 1f, float duration = 0f, int segments = 24)
        {
            float angle = 0f;
            Vector3 lastPoint = position + Quaternion.AngleAxis(angle, up) * Vector3.forward * radius;
            for (int i = 0; i <= segments; i++)
            {
                angle += 360f / segments;
                Vector3 nextPoint = position + Quaternion.AngleAxis(angle, up) * Vector3.forward * radius;
                Debug.DrawLine(lastPoint, nextPoint, color, duration);
                lastPoint = nextPoint;
            }
        }
    }

    void AcquireTargetFromVisible()
    {
        visibleTargets.RemoveAll(t => t == null || t.GetComponent<EnemyHealth>()?.isDead == true);

        if (visibleTargets.Count > 0)
        {
            target = visibleTargets[0];
            float minDist = Vector3.Distance(transform.position, target.position);
            foreach (var t in visibleTargets)
            {
                float d = Vector3.Distance(transform.position, t.position);
                if (d < minDist)
                {
                    minDist = d;
                    target = t;
                }
            }

            mode = AIMode.Engaging;
            turretLocked = false;
        }
    }

    bool TargetInvalid()
    {
        if (target == null) return true;
        var enemy = target.GetComponent<EnemyHealth>();
        return enemy == null || enemy.isDead;
    }

    void AbortEngagement()
    {
        turretLocked = false;
        mode = AIMode.Watching;
        target = null;
        firingLoopRunning = false;

        float currentAngle = turret.eulerAngles.y;
        float relativeAngle = Mathf.DeltaAngle(tankForwardAngle + scanCenterOffset, currentAngle);
        float targetOscillation = Mathf.Clamp(relativeAngle / (scanArcAngle / 2f), -1f, 1f);
        scanTimer = (Mathf.Asin(targetOscillation) / (Mathf.PI * 2f)) * scanPeriod;
        if (scanTimer < 0) scanTimer += scanPeriod;

        StopAllCoroutines();
        firingLoopRunning = false;
        isReloading = false;
        reloadTimer = 0f;

        StartCoroutine(ReturnToWatchingPosition());
    }

    private IEnumerator ReturnToWatchingPosition()
    {
        Quaternion startRot = turret.rotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            turret.rotation = Quaternion.Slerp(startRot, initialTurretRotation, t);
            yield return null;
        }
    }

    void ResetToWatching()
    {
        target = null;
        mode = AIMode.Watching;
        turretLocked = false;
        firingLoopRunning = false;
    }

    public void NotifyTargetEntered(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null && !enemy.isDead && !visibleTargets.Contains(other.transform))
            {
                visibleTargets.Add(other.transform);
                Debug.Log($"{name}: Detected enemy {other.name}");
            }
        }
    }

    public void NotifyTargetExited(Collider other)
    {
        if (visibleTargets.Contains(other.transform))
            visibleTargets.Remove(other.transform);

        if (target != null && other.transform == target)
        {
            Debug.Log($"{name}: Target left range {other.name}");
            ResetToWatching();
        }
    }
}


public class AITargetTrigger : MonoBehaviour
{
    private AITankTargeting ai;

    public void Setup(AITankTargeting aiTargeting)
    {
        ai = aiTargeting;
        if (!GetComponent<Collider>().isTrigger)
            GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        ai?.NotifyTargetEntered(other);
    }

    private void OnTriggerExit(Collider other)
    {
        ai?.NotifyTargetExited(other);
    }
}
