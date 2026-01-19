using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class SlantedHealthBar : VisualElement
{
    // 0..1
    [Range(0f, 1f)]
    public float value = 1f;

    // Theme
    public Color frameColor      = new Color(0.28f, 0.24f, 0.55f, 1f); // bright-ish purple (outer)
    public Color backgroundColor = new Color(0.12f, 0.10f, 0.28f, 1f); // mid/dark purple (inner bg)
    public Color capColor        = new Color(0.07f, 0.05f, 0.20f, 1f); // darker cap
    public Color fillColor       = new Color(0.85f, 0.10f, 0.55f, 1f); // pink

    // Thickness = how much bigger the frame layer is
    public float frameThickness = 6f;

    // Shape tuning
    public float leftSlant  = 26f; // top-left goes inward
    public float rightSlant = 22f; // controls right edge angles
    public float topLift    = 10f; // raises the top-right corner
    public float capWidth   = 64f; // fixed right segment

    // Fill end “cut” (diagonal end of the pink)
    public float fillCutMax = 18f;

    // Tiny bleed to kill 1px corner gaps (optional)
    public float fillLeftBleed = 1.0f;

    public SlantedHealthBar()
    {
        generateVisualContent += OnGenerateVisualContent;
        pickingMode = PickingMode.Ignore;
    }

    public void SetValue(float v)
    {
        value = Mathf.Clamp01(v);
        MarkDirtyRepaint();
    }

    public void SetTheme(Color fill, Color cap, Color frame, Color background)
    {
        fillColor       = fill;
        capColor        = cap;
        frameColor      = frame;
        backgroundColor = background;
        MarkDirtyRepaint();
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var p = mgc.painter2D;
        var r = contentRect;

        if (r.width <= 2f || r.height <= 2f)
            return;

        float pct = Mathf.Clamp01(value);

        p.lineJoin = LineJoin.Bevel;
        p.lineCap  = LineCap.Butt;

        // -------------------------------------------------
        // 1) OUTER FRAME (same shape, just "bigger")
        // -------------------------------------------------
        GetTrapezoid(r, -frameThickness, out var Af, out var Bf, out var Cf, out var Df);
        p.fillColor = frameColor;
        FillQuad(p, Af, Bf, Cf, Df);

        // -------------------------------------------------
        // 2) INNER BACKGROUND (same shape, smaller)
        // -------------------------------------------------
        GetTrapezoid(r, 0f, out var A, out var B, out var C, out var D);
        p.fillColor = backgroundColor;
        FillQuad(p, A, B, C, D);

        // Inner usable geometry
        float rightEdgeX = Mathf.Max(C.x, D.x);

        // -------------------------------------------------
        // 3) FILL (same as inner bg shape, clipped + cut)
        // -------------------------------------------------
        float usableRightX = rightEdgeX;
        float fillRightX   = Mathf.Lerp(A.x, usableRightX, pct);

        // If empty, don’t draw
        if (pct <= 0.0001f)
            return;

        // Fill starts EXACTLY on the inner trapezoid left edge
        Vector2 fillBL = A;
        Vector2 fillTL = B;

        float fillBotY = YOnLineAtX(A, D, fillRightX);

        Vector2 fillTR;
        Vector2 fillBR;

        if (pct >= 0.999f)
        {
            // At full, match the inner trapezoid's slanted right edge
            fillTR = C;
            fillBR = D;
        }
        else
        {
            // Diagonal cut at the fill end (top extends to the right a bit)
            float remaining = usableRightX - fillRightX;
            float cut       = Mathf.Min(fillCutMax, remaining * 0.75f);
            float cutX      = Mathf.Min(fillRightX + cut, usableRightX);

            float cutTopY = YOnLineAtX(B, C, cutX);

            fillTR = new Vector2(cutX, cutTopY);
            fillBR = new Vector2(fillRightX, fillBotY);
        }

        p.fillColor = fillColor;
        FillQuad(p, fillBL, fillTL, fillTR, fillBR);
    }

    // Builds the bar trapezoid for a given inset.
    // inset > 0 shrinks inward, inset < 0 grows outward.
    void GetTrapezoid(Rect baseRect, float inset,
        out Vector2 A, out Vector2 B, out Vector2 C, out Vector2 D)
    {
        Rect r = InsetRect(baseRect, inset);

        A = new Vector2(r.xMin, r.yMax);                       // bottom-left
        B = new Vector2(r.xMin + leftSlant, r.yMin);           // top-left (inset)
        C = new Vector2(
            r.xMax - rightSlant * 0.35f,
            r.yMin - topLift
        );                                                     // top-right (lifted)
        D = new Vector2(
            r.xMax - rightSlant * 1.00f,
            r.yMax
        );                                                     // bottom-right
    }

    static Rect InsetRect(Rect r, float inset)
    {
        // Positive inset = smaller rect. Negative inset = bigger rect.
        return new Rect(
            r.xMin + inset,
            r.yMin + inset,
            r.width  - inset * 2f,
            r.height - inset * 2f
        );
    }

    static void FillQuad(Painter2D p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        p.BeginPath();
        p.MoveTo(a);
        p.LineTo(b);
        p.LineTo(c);
        p.LineTo(d);
        p.ClosePath();
        p.Fill();
    }

    static float YOnLineAtX(Vector2 a, Vector2 b, float x)
    {
        float dx = b.x - a.x;
        if (Mathf.Abs(dx) < 0.0001f)
            return a.y;

        float t = (x - a.x) / dx;
        return Mathf.Lerp(a.y, b.y, t);
    }
}
