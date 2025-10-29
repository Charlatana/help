using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class TankMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardAcceleration = 20f;
    public float reverseAcceleration = 12f;
    public float maxForwardSpeed = 8f;
    public float maxReverseSpeed = 4f;
    public float brakeForce = 10f;

    [Header("Turning")]
    public float turnSpeed = 60f;

    [Header("Ground Check (Optional)")]
    public Transform groundCheck;
    public float groundCheckDistance = 1.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    // Debug
    private List<Vector3> trailPoints = new List<Vector3>();
    private Vector3 lastPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.None; // free rotation
        lastPos = transform.position;
        trailPoints.Add(lastPos);
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Optional ground check
        if (groundCheck != null)
        {
            isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
        }
        else
        {
            isGrounded = true; // no check = always grounded
        }

        // Add to trail when tank moves a bit
        if (Vector3.Distance(lastPos, transform.position) > 0.1f)
        {
            trailPoints.Add(transform.position);
            lastPos = transform.position;
        }

        // Draw all trail points (unlimited lifetime)
        for (int i = 1; i < trailPoints.Count; i++)
        {
            Debug.DrawLine(trailPoints[i - 1], trailPoints[i], isGrounded ? Color.green : Color.red);
        }
    }

    void FixedUpdate()
    {
        if (!isGrounded)
            return;

        // Forward/backward acceleration
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            float acceleration = moveInput > 0 ? forwardAcceleration : reverseAcceleration;
            rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, brakeForce * Time.fixedDeltaTime);
        }

        // Clamp forward/reverse speed
        float localSpeed = Vector3.Dot(rb.velocity, transform.forward);
        if (localSpeed > maxForwardSpeed)
            rb.velocity -= transform.forward * (localSpeed - maxForwardSpeed);
        else if (localSpeed < -maxReverseSpeed)
            rb.velocity -= transform.forward * (localSpeed + maxReverseSpeed);

        // Turning (independent of braking/acceleration, requires ground)
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float turnAmount = turnInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAmount, 0f));
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
    }
}
