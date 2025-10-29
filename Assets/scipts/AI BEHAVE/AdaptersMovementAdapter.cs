using UnityEngine;

[RequireComponent(typeof(TankMovement))]
public class MovementAdapter : MonoBehaviour, IMovement
{
    private TankMovement inner;
    private Rigidbody rb;

    public float moveSpeed = 6f; // used for MoveTo fallback
    public float rotateSpeed = 120f;

    void Awake()
    {
        inner = GetComponent<TankMovement>();
        rb = GetComponent<Rigidbody>();
    }

    public void MoveTo(Vector3 worldPos)
    {
        // naive direct move: compute direction and set velocity toward it
        if (rb == null) return;
        Vector3 dir = (worldPos - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.1f)
        {
            Stop();
            return;
        }
        dir.Normalize();
        // align body rotation then add forward force
        RotateTowards(worldPos);
        rb.velocity = transform.forward * moveSpeed;
    }

    public void Stop()
    {
        if (rb != null) rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.5f);
    }

    public void Reverse(float speed)
    {
        if (rb != null)
        {
            rb.velocity = -transform.forward * speed;
        }
    }

    public void RotateTowards(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        float step = rotateSpeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, dir.normalized, step * Mathf.Deg2Rad, 0f);
        transform.rotation = Quaternion.LookRotation(newDir);
    }
}
