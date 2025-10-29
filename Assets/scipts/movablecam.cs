using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;       // Speed of WASD movement
    public float heightSpeed = 5f;      // Speed for Q/E height adjustment
    public float rotationSpeed = 100f;  // Mouse drag rotation speed

    [Header("Height Limits")]
    public float minHeight = 5f;        // Prevents going underground
    public float maxHeight = 50f;       // Prevents flying too high

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // Get input
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // WASD movement in camera's forward/right direction
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        // Flatten movement so it doesn't move vertically
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h) * moveSpeed * Time.deltaTime;
        transform.position += moveDir;

        // Height control with Q and E
        float heightChange = 0f;
        if (Input.GetKey(KeyCode.E)) heightChange += heightSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q)) heightChange -= heightSpeed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y + heightChange, minHeight, maxHeight);
        transform.position = pos;
    }

    void HandleRotation()
    {
        // Rotate camera when holding right mouse button
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Rotate around Y (horizontal)
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);

            // Tilt up/down (local X axis)
            transform.Rotate(Vector3.right, -mouseY * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
