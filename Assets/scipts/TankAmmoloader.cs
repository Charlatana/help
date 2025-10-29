using UnityEngine;

public class TankAmmoSystem : MonoBehaviour
{
    [Header("Ammo Settings")]
    public AmmoType currentAmmo = AmmoType.APCR; // default
    public float reloadTime = 3f;
    private bool isReloading = false;

    // Event for UI updates if needed
    public System.Action<AmmoType> OnAmmoChanged;

    /// <summary>
    /// Select a new ammo type to use (callable from UI button)
    /// </summary>
    public void ChangeAmmo(string ammoName)
    {
        if (System.Enum.TryParse(ammoName, true, out AmmoType newAmmo))
        {
            currentAmmo = newAmmo;
            OnAmmoChanged?.Invoke(currentAmmo);
            Debug.Log($"{name} loaded ammo: {currentAmmo}");
        }
        else
        {
            Debug.LogWarning($"Invalid ammo name: {ammoName}");
        }
    }

    /// <summary>
    /// Fire ammo, applies reload delay automatically
    /// </summary>
    public bool CanFire()
    {
        return !isReloading;
    }

    public void StartReload()
    {
        if (!isReloading)
            StartCoroutine(ReloadCoroutine());
    }

    private System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log($"{name} reloading {currentAmmo}...");
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;
        Debug.Log($"{name} {currentAmmo} ready to fire!");
    }

    /// <summary>
    /// Returns the currently loaded ammo type
    /// </summary>
    public AmmoType GetCurrentAmmo()
    {
        return currentAmmo;
    }
}
