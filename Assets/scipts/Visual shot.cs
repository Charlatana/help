using UnityEngine;
using System.Collections;

public class VisualShotSystem : MonoBehaviour
{
    public static VisualShotSystem Instance;

    [Header("Visuals")]
    public GameObject shotPrefab;
    public GameObject hitEffectPrefab;

    [Header("Settings")]
    public float shotSpeed = 50f;
    public float shotLifetime = 2f;
    public float hitEffectLifetime = 2f; // ⏱️ how long hit visual stays

    public GunCaliberSystem gunCaliberSystem;

    private void Awake()
    {
        Instance = this;
    }

    public void Fire(Vector3 startPos, Vector3 targetPos, TankAmmoSystem ammoSystem)
    {
        if (shotPrefab == null)
        {
            Debug.LogWarning("No shotPrefab assigned!");
            return;
        }

        if (ammoSystem == null)
        {
            Debug.LogWarning("No TankAmmoSystem provided! Cannot determine ammo type.");
            return;
        }

        AmmoType ammoToFire = ammoSystem.GetCurrentAmmo();
        GameObject shot = Instantiate(shotPrefab, startPos, Quaternion.identity);

        // 🔄 Rotate shot to face direction
        Vector3 dir = (targetPos - startPos).normalized;
        shot.transform.rotation = Quaternion.LookRotation(dir);

        StartCoroutine(TravelShot(shot, startPos, targetPos, ammoSystem, ammoToFire));
    }

    private IEnumerator TravelShot(GameObject shot, Vector3 startPos, Vector3 targetPos, TankAmmoSystem ammoSystem, AmmoType ammoToFire)
    {
        float elapsed = 0f;
        float speed = shotSpeed;

        // 🔧 Speed modifiers by ammo type
        switch (ammoToFire)
        {
            case AmmoType.HE:
            case AmmoType.HEAT:
                speed *= 0.7f;
                break;
            case AmmoType.APHE:
                speed *= 0.85f;
                break;
            case AmmoType.APCR:
                speed *= 1.1f; // faster projectile
                break;
        }

        Vector3 dir = (targetPos - startPos).normalized;
        Vector3 velocity = dir * speed;

        // ⚙️ Gravity strength setup per ammo
        float gravityStrength = 0f;
        switch (ammoToFire)
        {
            case AmmoType.AP:         // straight
            case AmmoType.APCR:       // almost straight
                gravityStrength = 0f;
                break;
            case AmmoType.APHE:       // light arc
                gravityStrength = 9.81f * 0.2f;
                break;
            case AmmoType.HE:         // heavy falloff
            case AmmoType.HEAT:       // heavy falloff
                gravityStrength = 9.81f * 0.5f;
                break;
        }

        RaycastHit hitInfo = new RaycastHit(); // ✅ Initialize it properly
        bool hitSomething = false;

        while (elapsed < shotLifetime)
        {
            if (shot == null) yield break;

            velocity += Vector3.down * gravityStrength * Time.deltaTime;
            shot.transform.position += velocity * Time.deltaTime;

            if (velocity != Vector3.zero)
                shot.transform.rotation = Quaternion.LookRotation(velocity);

            // ✅ Raycast check
            if (Physics.Raycast(shot.transform.position, velocity.normalized, out hitInfo, speed * Time.deltaTime))
            {
                hitSomething = true;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Now hitInfo is guaranteed to exist
        Vector3 hitPoint = hitSomething ? hitInfo.point : shot.transform.position;
        Quaternion hitRot = hitSomething ? Quaternion.LookRotation(hitInfo.normal) : Quaternion.identity;
        // 💥 Hit visual
        if (hitEffectPrefab != null)
        {
            GameObject hitFX = Instantiate(hitEffectPrefab, hitPoint, hitRot);
            Destroy(hitFX, hitEffectLifetime);
        }

        // 🩸 Damage apply
        if (hitSomething && hitInfo.collider != null && hitInfo.collider.CompareTag("enemy"))
        {
            EnemyHealth enemy = hitInfo.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int baseDamage = AmmoData.GetDamage(ammoToFire, enemy.tankType);
                float finalDamage = baseDamage;

                GunCaliberSystem gunSystem = GetComponent<GunCaliberSystem>();
                if (gunSystem != null)
                    finalDamage *= gunSystem.GetCaliberMultiplier();

                enemy.TakeHit(Mathf.RoundToInt(finalDamage), ammoToFire, hitPoint);
            }
        }

        Destroy(shot);
        ammoSystem?.StartReload();
    }
}
