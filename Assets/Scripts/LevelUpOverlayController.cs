using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LevelUpOverlayController : MonoBehaviour
{
    [SerializeField] PlayerXP player;
    [SerializeField] bool pauseOnLevelUp = true;

    VisualElement overlay;
    VisualElement panel;
    Button continueButton;

    bool isOpen;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        overlay = root.Q<VisualElement>("levelUpOverlay");
        panel = root.Q<VisualElement>("levelUpPanel");
        continueButton = root.Q<Button>("continueButton");

        if (continueButton != null)
            continueButton.clicked += Close;

        if (player != null)
            player.LeveledUp += Open;

        // Start hidden
        if (overlay != null) overlay.style.display = DisplayStyle.None;

        // Good defaults for animation start state
        SetAnimState(open: false, instant: true);
    }

    void OnDisable()
    {
        if (continueButton != null)
            continueButton.clicked -= Close;

        if (player != null)
            player.LeveledUp -= Open;

        if (isOpen) Resume();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.lKey.wasPressedThisFrame)
            Open();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    void Open()
    {
        if (overlay == null || panel == null) return;

        isOpen = true;
        overlay.style.display = DisplayStyle.Flex;

        if (pauseOnLevelUp) Time.timeScale = 0f;

        // Start from hidden state, then animate to open
        SetAnimState(open: false, instant: true);
        AnimateIn();

    }

    void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        AnimateOut();
    }

    // ---------- Animation helpers ----------

    void SetAnimState(bool open, bool instant)
    {
        // Overlay fade
        if (overlay != null)
            overlay.style.opacity = open ? 1f : 0f;

        // Panel pop
        if (panel != null)
        {
            panel.style.opacity = open ? 1f : 0f;
            panel.style.scale = open ? new Scale(new Vector2(1f, 1f)) : new Scale(new Vector2(0.92f, 0.92f));
        }

        // If instant, force a repaint now
        if (instant)
        {
            overlay?.MarkDirtyRepaint();
            panel?.MarkDirtyRepaint();
        }
    }

    void AnimateIn()
    {
        // 0.18s-ish feels snappy
        Animate(0.18f, t =>
        {
            float ease = EaseOutCubic(t);

            if (overlay != null) overlay.style.opacity = Mathf.Lerp(0f, 1f, ease);
            if (panel != null)
            {
                panel.style.opacity = Mathf.Lerp(0f, 1f, ease);
                float s = Mathf.Lerp(0.92f, 1f, ease);
                panel.style.scale = new Scale(new Vector2(s, s));
            }
        });
    }

    void AnimateOut()
    {
        Animate(0.12f, t =>
        {
            float ease = EaseInCubic(t);

            if (overlay != null) overlay.style.opacity = Mathf.Lerp(1f, 0f, ease);
            if (panel != null)
            {
                panel.style.opacity = Mathf.Lerp(1f, 0f, ease);
                float s = Mathf.Lerp(1f, 0.95f, ease);
                panel.style.scale = new Scale(new Vector2(s, s));
            }
        },
        onDone: () =>
        {
            if (overlay != null) overlay.style.display = DisplayStyle.None;
            Resume();
        });
    }

    void Resume()
    {
        if (pauseOnLevelUp) Time.timeScale = 1f;
    }

    void Animate(float duration, System.Action<float> step, System.Action onDone = null)
    {
        float elapsed = 0f;
        IVisualElementScheduledItem item = null;

        item = overlay.schedule.Execute(() =>
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);

            step?.Invoke(t);

            if (t >= 1f)
            {
                item.Pause();
                onDone?.Invoke();
            }
        }).Every(16);
    }

    static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    static float EaseInCubic(float t) => t * t * t;
}
