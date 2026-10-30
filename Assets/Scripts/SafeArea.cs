using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform rt;
    private Rect applied;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (applied != Screen.safeArea) Apply();
    }

    private void Apply()
    {
        applied = Screen.safeArea;
        Vector2 min = applied.position;
        Vector2 max = applied.position + applied.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}