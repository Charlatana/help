using UnityEngine;

public class GunCaliberSystem : MonoBehaviour
{
    public enum GunCaliber
    {
        mm75,
        mm80,
        mm88,
        mm100,
        mm120
    }

    [Header("Current Gun Caliber")]
    public GunCaliber currentCaliber = GunCaliber.mm80;

    public float GetCaliberMultiplier()
    {
        switch (currentCaliber)
        {
            case GunCaliber.mm75: return 0.8f;
            case GunCaliber.mm80: return 1.0f;
            case GunCaliber.mm88: return 1.1f;
            case GunCaliber.mm100: return 1.2f;
            case GunCaliber.mm120: return 1.4f;
            default: return 1.0f;
        }
    }

    public void SetCaliber(int caliberIndex)
    {
        currentCaliber = (GunCaliber)caliberIndex;
        Debug.Log($"📢 Caliber changed to: {currentCaliber}, Multiplier: {GetCaliberMultiplier()}");
    }
}
