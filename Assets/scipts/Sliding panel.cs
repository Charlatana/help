using UnityEngine;
using UnityEngine.UI;

public class SlidingPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    public RectTransform panel; // Assign your TravelerPanel here
    public Vector2 hiddenPosition;   // Off-screen position
    public Vector2 visiblePosition;  // On-screen position
    public float slideSpeed = 10f;

    private bool isOpen = false;
    private bool isSliding = false;

    /// <summary>
    /// Called by button to toggle panel
    /// </summary>
    public void TogglePanel()
    {
        if (isSliding) return; // Ignore clicks during animation
        isOpen = !isOpen;
        StartCoroutine(SlidePanel());
    }

    private System.Collections.IEnumerator SlidePanel()
    {
        isSliding = true;
        Vector2 target = isOpen ? visiblePosition : hiddenPosition;

        // Slide smoothly
        while (Vector2.Distance(panel.anchoredPosition, target) > 0.1f)
        {
            panel.anchoredPosition = Vector2.Lerp(
                panel.anchoredPosition,
                target,
                Time.deltaTime * slideSpeed
            );
            yield return null;
        }

        // Snap exactly to target at the end
        panel.anchoredPosition = target;
        isSliding = false;
    }
}
