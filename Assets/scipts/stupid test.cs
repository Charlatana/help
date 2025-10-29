using UnityEngine;

public class SimpleDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DETECTOR] Entered by: {other.name}");
    }


    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[DETECTOR] Exited: {other.name}");
    }
}