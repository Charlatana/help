using UnityEngine;

public enum TankType
{
    Light,
    Armoured,
    Heavy,
    Niggers
}

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public TankType tankType = TankType.Light;
    public float maxHealth = 1500f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Death Settings")]
    public GameObject explosionPrefab;   // optional explosion effect
    public float destroyDelay = 5f;      // how long before the object is cleaned up

    [Header("UI Settings")]
    public GameObject damageIndicatorPrefab;
    public Transform uiAnchor;  // Optional – assign a top point of tank for UI positioning

    public EnemyHealthBar healthBarPrefab;
    private EnemyHealthBar barInstance;
    private void Start()
    {
        // Adjust HP depending on tank type
        switch (tankType)
        {
            case TankType.Light: maxHealth = 1000f; break;
            case TankType.Armoured: maxHealth = 1500f; break;
            case TankType.Heavy: maxHealth = 2500f; break;
            case TankType.Niggers: maxHealth = 100000f; break;
        }

        // Adjust HP depending on tank type...
        currentHealth = maxHealth;

        // Spawn health bar
        if (healthBarPrefab != null)
        {
            barInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            barInstance.Init(this);
        }
    }

    public void TakeHit(int damage, AmmoType ammoType, Vector3 hitPoint)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Spawn floating damage text at hit point
        DamageIndicator.Create(damage, hitPoint, 3f, 3f, 3f);

        Debug.Log($"{name} hit by {ammoType}! Damage: {damage}, Remaining HP: {Mathf.Max(currentHealth, 0)}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{name} is destroyed!");

        // Explosion FX
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Disable visuals and collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;

        // Optional: disable AI components
        AITankTargeting ai = GetComponent<AITankTargeting>();
        if (ai != null) ai.enabled = false;

        // Destroy after delay
        Destroy(gameObject, destroyDelay);
    }

    public bool IsDead()
    {
        return isDead;
    }
}
