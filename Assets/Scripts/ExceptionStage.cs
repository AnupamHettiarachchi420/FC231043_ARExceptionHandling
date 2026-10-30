using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExceptionStage : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private Transform stageRoot;
    [SerializeField] private GameObject framePrefab;
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private TMP_Text statusText;

    [Header("Layout (metres)")]
    [SerializeField] private float frameWidth = 0.18f;
    [SerializeField] private float frameHeight = 0.05f;
    [SerializeField] private float frameDepth = 0.10f;
    [SerializeField] private float gap = 0.006f;
    [SerializeField] private float baseHeight = 0.012f;
    [SerializeField] private float tokenSize = 0.035f;

    private class Frame
    {
        public GameObject root;
        public Renderer body;
        public Color color;
    }

    private readonly List<Frame> frames = new();
    private readonly List<string> output = new();
    private GameObject token;
    private TMP_Text tokenLabel;
    private bool busy;
    private Camera cam;

    private static readonly Color ErrorRed = new Color(1f, 0.45f, 0.2f);
    private static readonly Color CatchGreen = new Color(0.3f, 0.67f, 0.97f);
    private static readonly Color FinallyBlue = new Color(0.75f, 0.65f, 1f);
    private static readonly Color CrashDark = new Color(0.45f, 0.08f, 0.08f);

    public bool IsBusy => busy;

    // ---------- Public API ----------

    public void ResetStage(string rootLabel, bool rootHasTry)
    {
        StopAllCoroutines();
        busy = false;
        RemoveToken();
        for (int i = stageRoot.childCount - 1; i >= 0; i--)
        {
            Transform c = stageRoot.GetChild(i);
            if (c.name.StartsWith("Frame") || c.name.StartsWith("Token"))
                Destroy(c.gameObject);
        }
        frames.Clear();
        output.Clear();

        Frame root = CreateFrame(rootLabel, rootHasTry, 0);
        root.root.transform.localPosition = SlotCenter(0);
        frames.Add(root);
    }

    public void Say(string message) { Say(message, Color.white); }

    public void Say(string message, Color color)
    {
        string outText = output.Count == 0 ? "(nothing yet)" : string.Join("  ", output);
        statusText.text = message + "\n<color=#8FD3FF>Output: " + outText + "</color>";
        statusText.color = color;
    }

    public void Play(LessonStep step, string prefix = "")
    {
        StartCoroutine(PlayRoutine(step, prefix));
    }

    // Applies a step without animation (used by Back)
    public void ApplyInstant(LessonStep step)
    {
        switch (step.kind)
        {
            case StepKind.Print:
                output.Add(step.arg);
                break;
            case StepKind.Call:
                {
                    int index = frames.Count;
                    Frame f = CreateFrame(step.arg, step.hasTry, index);
                    f.root.transform.localPosition = SlotCenter(index);
                    frames.Add(f);
                    break;
                }
            case StepKind.Return:
                RemoveTopFrame();
                break;
            case StepKind.Throw:
                if (frames.Count > 0)
                {
                    CreateToken(step.arg, TokenSpot(frames.Count - 1));
                    frames[frames.Count - 1].body.material.color = ErrorRed;
                }
                break;
            case StepKind.Unwind:
                RemoveTopFrame();
                if (token != null) token.transform.localPosition = TokenSpot(frames.Count - 1);
                break;
            case StepKind.Catch:
                RemoveToken();
                break;
            case StepKind.Crash:
                foreach (Frame f in frames) f.body.material.color = CrashDark;
                break;
        }
    }

    // ---------- Animated steps ----------

    private IEnumerator PlayRoutine(LessonStep step, string prefix)
    {
        busy = true;
        switch (step.kind)
        {
            case StepKind.Print:
                {
                    output.Add(step.arg);
                    Say(prefix + step.message);
                    if (frames.Count > 0)
                    {
                        Transform t = frames[frames.Count - 1].body.transform;
                        Vector3 s = t.localScale;
                        yield return StartCoroutine(Tween(0.4f, e =>
                            t.localScale = s * (1f + 0.12f * Mathf.Sin(Mathf.PI * e))));
                        t.localScale = s;
                    }
                    break;
                }
            case StepKind.Call:
                {
                    int index = frames.Count;
                    Frame f = CreateFrame(step.arg, step.hasTry, index);
                    frames.Add(f);
                    Say(prefix + step.message);
                    Vector3 target = SlotCenter(index);
                    Vector3 start = target + Vector3.up * 0.25f;
                    f.root.transform.localPosition = start;
                    f.root.transform.localScale = Vector3.zero;
                    yield return StartCoroutine(Tween(0.5f, e =>
                    {
                        f.root.transform.localPosition = Vector3.Lerp(start, target, e);
                        f.root.transform.localScale = Vector3.one * e;
                    }));
                    break;
                }
            case StepKind.Return:
                {
                    if (frames.Count > 0)
                    {
                        Frame f = frames[frames.Count - 1];
                        frames.RemoveAt(frames.Count - 1);
                        Say(prefix + step.message);
                        Vector3 from = f.root.transform.localPosition;
                        Vector3 to = from + Vector3.up * 0.2f;
                        yield return StartCoroutine(Tween(0.45f, e =>
                        {
                            f.root.transform.localPosition = Vector3.Lerp(from, to, e);
                            f.root.transform.localScale = Vector3.one * (1f - e);
                        }));
                        Destroy(f.root);
                    }
                    break;
                }
            case StepKind.Throw:
                {
                    if (frames.Count > 0)
                    {
                        Frame top = frames[frames.Count - 1];
                        Color orig = top.body.material.color;
                        CreateToken(step.arg, TokenSpot(frames.Count - 1));
                        token.transform.localScale = Vector3.zero;
                        Say(prefix + step.message, new Color(1f, 0.55f, 0.55f));
                        yield return StartCoroutine(Tween(0.5f, e =>
                        {
                            token.transform.localScale = Vector3.one * (tokenSize * e);
                            top.body.material.color = Color.Lerp(orig, ErrorRed, e);
                        }));
                    }
                    break;
                }
            case StepKind.Unwind:
                {
                    if (frames.Count > 0 && token != null)
                    {
                        Frame gone = frames[frames.Count - 1];
                        frames.RemoveAt(frames.Count - 1);
                        Say(prefix + step.message);
                        Vector3 from = token.transform.localPosition;
                        Vector3 to = TokenSpot(frames.Count - 1);
                        yield return StartCoroutine(Tween(0.7f, e =>
                        {
                            token.transform.localPosition = Vector3.Lerp(from, to, e);
                            gone.root.transform.localScale = Vector3.one * (1f - e);
                        }));
                        Destroy(gone.root);
                    }
                    break;
                }
            case StepKind.Catch:
                {
                    if (frames.Count > 0 && token != null)
                    {
                        Frame f = frames[frames.Count - 1];
                        Say(prefix + step.message, new Color(0.55f, 1f, 0.7f));
                        Vector3 from = token.transform.localPosition;
                        Vector3 to = SlotCenter(frames.Count - 1);
                        Color start = f.body.material.color;
                        yield return StartCoroutine(Tween(0.6f, e =>
                        {
                            token.transform.localPosition = Vector3.Lerp(from, to, e);
                            token.transform.localScale = Vector3.one * (tokenSize * (1f - e));
                            f.body.material.color = Color.Lerp(start, CatchGreen, e);
                        }));
                        RemoveToken();
                        yield return StartCoroutine(Tween(0.4f, e =>
                            f.body.material.color = Color.Lerp(CatchGreen, f.color, e)));
                    }
                    break;
                }
            case StepKind.Finally:
                {
                    Say(prefix + step.message);
                    if (frames.Count > 0)
                    {
                        Frame f = frames[frames.Count - 1];
                        yield return StartCoroutine(Tween(0.6f, e =>
                            f.body.material.color =
                                Color.Lerp(f.color, FinallyBlue, Mathf.Sin(Mathf.PI * e))));
                    }
                    break;
                }
            case StepKind.Crash:
                {
                    Say(prefix + step.message, new Color(1f, 0.55f, 0.55f));
                    var snapshot = new List<Frame>(frames);
                    yield return StartCoroutine(Tween(0.8f, e =>
                    {
                        foreach (Frame f in snapshot)
                            f.body.material.color = Color.Lerp(f.color, CrashDark, e);
                        if (token != null)
                            token.transform.localScale =
                                Vector3.one * (tokenSize * (1f + 0.4f * Mathf.Sin(Mathf.PI * e)));
                    }));
                    break;
                }
            default:
                {
                    Say(prefix + step.message);
                    yield return new WaitForSeconds(0.2f);
                    break;
                }
        }
        busy = false;
    }

    private static IEnumerator Tween(float duration, System.Action<float> apply)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            apply(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
            yield return null;
        }
        apply(1f);
    }

    // ---------- Helpers ----------

    private void LateUpdate()
    {
        if (token == null || tokenLabel == null) return;
        if (cam == null) cam = Camera.main;
        tokenLabel.transform.position =
            token.transform.position + Vector3.up * (tokenSize * 0.5f + 0.03f);
        tokenLabel.transform.localScale = Vector3.one * (token.transform.localScale.x / tokenSize);
        if (cam != null)
            tokenLabel.transform.rotation =
                Quaternion.LookRotation(tokenLabel.transform.position - cam.transform.position);
    }

    private Vector3 SlotCenter(int index)
    {
        return new Vector3(0f, baseHeight + frameHeight / 2f + index * (frameHeight + gap), 0f);
    }

    private Vector3 TokenSpot(int frameIndex)
    {
        float y = frameIndex < 0
            ? baseHeight + tokenSize / 2f + 0.01f
            : SlotCenter(frameIndex).y + frameHeight / 2f + tokenSize / 2f + 0.01f;
        return new Vector3(0f, y, 0f);
    }

    private Frame CreateFrame(string label, bool hasTry, int index)
    {
        var f = new Frame();
        f.root = new GameObject("Frame " + label);
        f.root.transform.SetParent(stageRoot, false);

        GameObject body = Instantiate(framePrefab, f.root.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(frameWidth, frameHeight, frameDepth);
        f.body = body.GetComponent<Renderer>();
        f.color = hasTry
            ? new Color(0.2f, 0.78f, 0.75f)
            : Color.HSVToRGB(Mathf.Repeat(0.58f + index * 0.08f, 1f), 0.55f, 1f);
        f.body.material.color = f.color;

        AddFrameLabel(f.root.transform, label, hasTry, 0f);
        AddFrameLabel(f.root.transform, label, hasTry, 180f);
        return f;
    }

    private void AddFrameLabel(Transform parent, string label, bool hasTry, float yAngle)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        Quaternion rot = Quaternion.Euler(0f, yAngle, 0f);
        go.transform.localRotation = rot;
        go.transform.localPosition = rot * new Vector3(0f, 0f, -(frameDepth / 2f + 0.001f));

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = hasTry ? label + "\n<size=60%>try { }</size>" : label;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.04f;
        tmp.fontSizeMax = 0.25f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.06f, 0.09f, 0.16f);
        tmp.rectTransform.sizeDelta = new Vector2(frameWidth - 0.01f, frameHeight - 0.005f);
    }

    private void CreateToken(string typeName, Vector3 localPos)
    {
        RemoveToken();
        token = Instantiate(tokenPrefab, stageRoot);
        token.name = "Token";
        token.transform.localPosition = localPos;
        token.transform.localScale = Vector3.one * tokenSize;
        token.GetComponent<Renderer>().material.color = ErrorRed;

        var go = new GameObject("TokenLabel");
        go.transform.SetParent(stageRoot, false);
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = typeName;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.04f;
        tmp.fontSizeMax = 0.2f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.55f, 0.55f);
        tmp.rectTransform.sizeDelta = new Vector2(0.30f, 0.05f);
        tokenLabel = tmp;
    }

    private void RemoveToken()
    {
        if (token != null) Destroy(token);
        if (tokenLabel != null) Destroy(tokenLabel.gameObject);
        token = null;
        tokenLabel = null;
    }

    private void RemoveTopFrame()
    {
        if (frames.Count == 0) return;
        Destroy(frames[frames.Count - 1].root);
        frames.RemoveAt(frames.Count - 1);
    }
}