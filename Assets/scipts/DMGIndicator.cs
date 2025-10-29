using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveUpSpeed = 1f;
    public float lifetime = 3f;
    public float fadeDuration = 0.5f;

    [Header("Visual Settings")]
    public float textSize = 3f;
    public Color textColor = Color.red;
    public float verticalOffset = 1f; // adjustable offset above hit

    private float timer;
    private TextMeshProUGUI tmpText;
    private Canvas canvas;
    private Color startColor;

    public static void Create(int damage, Vector3 position, float life = 1f, float fade = 0.5f, float offset = 0.5f)
    {
        GameObject obj = new GameObject("DamageIndicator");
        var indicator = obj.AddComponent<DamageIndicator>();
        indicator.lifetime = life;
        indicator.fadeDuration = fade;
        indicator.verticalOffset = offset;
        indicator.SetupText(damage, position);
    }

    private void SetupText(int damage, Vector3 pos)
    {
        pos += Vector3.up * verticalOffset; // apply offset

        // World-space canvas
        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform cRect = canvas.GetComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(200, 100);
        cRect.localScale = Vector3.one * 0.01f;

        // TMP text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform);
        tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontSize = textSize;
        tmpText.color = textColor;
        tmpText.text = damage.ToString();

        RectTransform tRect = tmpText.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(200, 100);
        tRect.localPosition = Vector3.zero;

        transform.position = pos;
        startColor = tmpText.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Move upward
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        // ✅ Always face camera
        if (Camera.main)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // Fade out
        if (timer > lifetime - fadeDuration)
        {
            float fade = 1f - ((timer - (lifetime - fadeDuration)) / fadeDuration);
            tmpText.color = new Color(startColor.r, startColor.g, startColor.b, fade);
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
