using UnityEngine;
using System;

[RequireComponent(typeof(EnemyHealth))]
public class HealthAdapter : MonoBehaviour, IHealth
{
    EnemyHealth inner;
    public event Action<float> OnHealthChanged;

    void Awake()
    {
        inner = GetComponent<EnemyHealth>();
    }

    void Start()
    {
        // if inner doesn't provide an event, we poll in Update (cheap)
        OnHealthChanged?.Invoke(Percent);
    }

    float lastPercent = -1f;
    void Update()
    {
        if (inner == null) return;
        float p = Percent;
        if (Mathf.Abs(p - lastPercent) > 0.001f)
        {
            lastPercent = p;
            OnHealthChanged?.Invoke(p);
        }
    }

    public float Current => inner != null ? inner.currentHealth : 0f;
    public float Max => inner != null ? inner.maxHealth : 1f;
    public bool IsDead => inner != null ? inner.isDead : true;
    public float Percent => Max > 0f ? Mathf.Clamp01(Current / Max) : 0f;
}
