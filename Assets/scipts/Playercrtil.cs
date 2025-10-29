using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Net.NetworkInformation;

public class PlayerTankController : MonoBehaviour
{
    [Header("References")]
    public Transform turret;
    public Transform firePoint;
    public Camera playerCam;
    public Camera mainCam;
    public TextMeshProUGUI reloadText;

    [Header("Control Settings")]
    public float mouseSensitivity = 50f;
    public float verticalLimit = 25f;
    public float reloadTime = 3f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool canFire = true;
    private bool activeControl = false;

    private AITankTargeting aiSystem;
    public TankAmmoSystem ammoSystem;
    private GunCaliberSystem gunSystem; 

    private void Start()
    {
        aiSystem = GetComponent<AITankTargeting>();
        gunSystem = GetComponent<GunCaliberSystem>(); 

        if (reloadText != null) reloadText.gameObject.SetActive(false);

        if (playerCam != null) playerCam.enabled = false;
        if (mainCam != null) mainCam.enabled = true;
    }

    private void Update()
    {
        if (!activeControl) return;

        HandleTurretLook();

        if (Input.GetMouseButtonDown(0) && canFire)
            StartCoroutine(Fire());

        if (Input.GetKeyDown(KeyCode.Escape))
            ExitPlayerControl();
    }

    void HandleTurretLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate turret (yaw)
        yRotation += mouseX;
        turret.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        // Rotate barrel (pitch)
        if (turret != null && turret.childCount > 0)
        {
            if (firePoint == null) return;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

            // Apply pitch to barrel, not turret
            if (firePoint.parent != null)
            {
                Transform barrel = firePoint.parent;
                barrel.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }
    }



    IEnumerator Fire()
    {
        canFire = false;

        Vector3 dir = firePoint.forward;
        Vector3 impactPoint = firePoint.position + dir * 100f;
        if (Physics.Raycast(firePoint.position, dir, out RaycastHit hit, 100f))
            impactPoint = hit.point;

        Debug.DrawLine(firePoint.position, impactPoint, Color.green, 1f);
        VisualShotSystem.Instance.Fire(firePoint.position, impactPoint, ammoSystem);

        if (Physics.Raycast(firePoint.position, dir, out hit, 100f) && hit.collider.CompareTag("enemy"))
        {
            var ammoSystem = GetComponent<TankAmmoSystem>();
            if (ammoSystem != null && ammoSystem.CanFire())
            {
                AmmoType ammoToFire = ammoSystem.GetCurrentAmmo();

                var enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    int dmg = AmmoData.GetDamage(ammoToFire, enemy.tankType);

                    if (gunSystem != null)
                        dmg = Mathf.RoundToInt(dmg * gunSystem.GetCaliberMultiplier());

                    enemy.TakeHit(dmg, ammoToFire, hit.point);
                    Debug.Log($"[{ammoToFire}] ({gunSystem?.currentCaliber}) dealt {dmg} dmg. Remaining HP: {enemy.currentHealth:F0}");
                }

                ammoSystem.StartReload();
            }
        }

        if (reloadText != null)
        {
            reloadText.text = "Reloading...";
            reloadText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(reloadTime);

        canFire = true;
        if (reloadText != null)
        {
            reloadText.text = "Ready!";
            yield return new WaitForSeconds(0.5f);
            reloadText.gameObject.SetActive(false);
        }
    }

    public void EnterPlayerControl()
    {
        if (aiSystem != null)
            aiSystem.enabled = false;

        activeControl = true;

        if (playerCam != null) playerCam.enabled = true;
        if (mainCam != null) mainCam.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitPlayerControl()
    {
        if (aiSystem != null)
            aiSystem.enabled = true;

        activeControl = false;

        if (playerCam != null) playerCam.enabled = false;
        if (mainCam != null) mainCam.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
