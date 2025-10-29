using UnityEngine;
using System.Collections.Generic;

public class AIMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 5f;
    public float topSpeed = 10f;
    public float accelerationTime = 2f; // seconds to reach top speed

    [Header("Path Settings")]
    public List<Transform> waypoints = new List<Transform>();
    public float arriveThreshold = 0.5f; // stop distance from waypoint

    private int currentWaypoint = 0;
    private float currentSpeed = 0f;
    private float accelerationRate;
    private bool moving = false;

    void Start()
    {
        accelerationRate = (topSpeed - normalSpeed) / accelerationTime;
    }

    void Update()
    {
        if (!moving || waypoints.Count == 0) return;

        Transform target = waypoints[currentWaypoint];
        Vector3 direction = (target.position - transform.position).normalized;

        // Accelerate toward top speed
        currentSpeed = Mathf.MoveTowards(currentSpeed, topSpeed, accelerationRate * Time.deltaTime);

        transform.position += direction * currentSpeed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 5f); // optional facing

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < arriveThreshold)
        {
            currentWaypoint++;
            currentSpeed = normalSpeed; // reset to normal speed each new waypoint
            if (currentWaypoint >= waypoints.Count)
            {
                moving = false;
                currentWaypoint = 0;
            }
        }
    }

    public void StartMovement(List<Transform> newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypoint = 0;
        currentSpeed = normalSpeed;
        moving = true;
    }

    public void MoveToSingle(Vector3 targetPos)
    {
        GameObject temp = new GameObject("TempTarget");
        temp.transform.position = targetPos;
        waypoints.Clear();
        waypoints.Add(temp.transform);
        currentWaypoint = 0;
        currentSpeed = normalSpeed;
        moving = true;
    }
}
