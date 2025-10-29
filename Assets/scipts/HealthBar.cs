using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider slider;
    public CanvasGroup canvasGroup; // optional for fade
    public Vector3 offset = new Vector3(0, 3f, 0);

    [Header("Settings")]
    public float smoothSpeed = 5f;
    public float fadeSpeed = 3f;

    private Camera cam;
    private Transform target;
    private EnemyHealth enemy;

    public void Init(EnemyHealth e)
    {
        enemy = e;
        target = e.transform;
        slider.maxValue = e.maxHealth;
        slider.value = e.maxHealth;
        cam = Camera.main;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    void Update()
    {
        if (enemy == null)
        {
            Destroy(gameObject);
            return;
        }

        // Smooth HP bar update
        slider.value = Mathf.Lerp(slider.value, enemy.currentHealth, Time.deltaTime * smoothSpeed);

        // Follow enemy
        transform.position = target.position + offset;

        // Face the camera
        if (cam)
        {
            Vector3 dir = transform.position - cam.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Optional fade out when dead
        if (canvasGroup != null)
        {
            float targetAlpha = enemy.IsDead() ? 0f : 1f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
}
