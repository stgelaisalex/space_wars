using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class XPBar : VisualElement
{
    // 0..1
    [Range(0f, 1f)]
    public float value
    {
        get => _value;
        set
        {
            float v = Mathf.Clamp01(value);
            if (Mathf.Approximately(_value, v)) return;
            _value = v;
            MarkDirtyRepaint();
        }
    }

    [Range(0f, 60f)] public float slant = 18f;
    [Range(0f, 10f)] public float frameThickness = 7f;

    public Color frameColor = new Color(0.10f, 0.10f, 0.30f, 1f);
    public Color bgColor    = new Color(0.06f, 0.06f, 0.18f, 1f);
    public Color fillColor  = new Color(0.95f, 0.45f, 0.10f, 1f);

    float _value = 0f;

    public XPBar()
    {
        // Default size so it shows up in UI Builder even without styles
        style.height = 28;
        style.flexGrow = 1;

        generateVisualContent += OnGenerateVisualContent;
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var p = ctx.painter2D;
        float inset = frameThickness;
        float w = contentRect.width;
        float h = contentRect.height;
        if (w <= 1f || h <= 1f) return;

        float s = Mathf.Clamp(slant, 0f, w * 0.45f);

        // --- Draw background (parallelogram) ---
        p.fillColor = bgColor;
        p.BeginPath();
        p.MoveTo(new Vector2(s, 0));
        p.LineTo(new Vector2(w, 0));
        p.LineTo(new Vector2(w - s, h));
        p.LineTo(new Vector2(0, h));
        p.ClosePath();
        p.Fill();

        // --- Draw fill ---
        float fillW = Mathf.Clamp01(_value) * w;
        float innerW = Mathf.Max(0, fillW - inset);

        if (innerW > inset)
        {
            p.fillColor = fillColor;
            p.BeginPath();

            p.MoveTo(new Vector2(s + inset, inset));
            p.LineTo(new Vector2(innerW - inset, inset));
            p.LineTo(new Vector2(innerW - s - inset, h - inset));
            p.LineTo(new Vector2(inset, h - inset));

            p.ClosePath();
            p.Fill();
        }

        // --- Draw frame (stroke) ---
        p.strokeColor = frameColor;
        p.lineWidth = frameThickness;

        p.BeginPath();
        p.MoveTo(new Vector2(s, 0));
        p.LineTo(new Vector2(w, 0));
        p.LineTo(new Vector2(w - s, h));
        p.LineTo(new Vector2(0, h));
        p.ClosePath();
        p.Stroke();
    Debug.Log($"XPBar draw: w={contentRect.width} h={contentRect.height} value={_value}");
    }
}
