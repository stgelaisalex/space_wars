using UnityEngine;

public class MapBounds : MonoBehaviour
{
    [Header("Who to constrain")]
    public Transform target; // Ship

    [Header("Playable bounds (world units) on XZ plane")]
    public Vector2 centerXZ = Vector2.zero;      // (x,z)
    public Vector2 sizeXZ = new Vector2(160f, 120f); // (width in x, height in z)

    [Header("Visual danger band thickness (world units)")]
    public float bandThickness = 8f;

    [Header("Visuals (optional)")]
    public Transform topBand;    // +Z
    public Transform bottomBand; // -Z
    public Transform leftBand;   // -X
    public Transform rightBand;  // +X

    void LateUpdate()
    {
        ConstrainTargetXZ();
        UpdateBandsXZ();
    }

    void ConstrainTargetXZ()
    {
        if (!target) return;

        float halfW = sizeXZ.x * 0.5f;
        float halfH = sizeXZ.y * 0.5f;

        Vector3 p = target.position;
        p.x = Mathf.Clamp(p.x, centerXZ.x - halfW, centerXZ.x + halfW);
        p.z = Mathf.Clamp(p.z, centerXZ.y - halfH, centerXZ.y + halfH);
        target.position = p;
    }

    void UpdateBandsXZ()
    {
        float halfW = sizeXZ.x * 0.5f;
        float halfH = sizeXZ.y * 0.5f;

        // Bands are thin rectangles on the XZ plane.
        // Quad default faces +Z, so rotate them to lie flat on XZ.
        Quaternion flat = Quaternion.Euler(90f, 0f, 0f);

        if (topBand)
        {
            topBand.rotation = flat;
            topBand.position = new Vector3(centerXZ.x, topBand.position.y, centerXZ.y + (halfH - bandThickness * 0.5f));
            topBand.localScale = new Vector3(sizeXZ.x, bandThickness, 1f);
        }

        if (bottomBand)
        {
            bottomBand.rotation = flat;
            bottomBand.position = new Vector3(centerXZ.x, bottomBand.position.y, centerXZ.y - (halfH - bandThickness * 0.5f));
            bottomBand.localScale = new Vector3(sizeXZ.x, bandThickness, 1f);
        }

        if (leftBand)
        {
            leftBand.rotation = flat;
            leftBand.position = new Vector3(centerXZ.x - (halfW - bandThickness * 0.5f), leftBand.position.y, centerXZ.y);
            leftBand.localScale = new Vector3(bandThickness, sizeXZ.y - bandThickness * 2f, 1f);
        }

        if (rightBand)
        {
            rightBand.rotation = flat;
            rightBand.position = new Vector3(centerXZ.x + (halfW - bandThickness * 0.5f), rightBand.position.y, centerXZ.y);
            rightBand.localScale = new Vector3(bandThickness, sizeXZ.y - bandThickness * 2f, 1f);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);

        float halfW = sizeXZ.x * 0.5f;
        float halfH = sizeXZ.y * 0.5f;

        Vector3 c = new Vector3(centerXZ.x, transform.position.y, centerXZ.y);

        Vector3 a = c + new Vector3(-halfW, 0f, -halfH);
        Vector3 b = c + new Vector3(-halfW, 0f,  halfH);
        Vector3 d = c + new Vector3( halfW, 0f, -halfH);
        Vector3 e = c + new Vector3( halfW, 0f,  halfH);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, e);
        Gizmos.DrawLine(e, d);
        Gizmos.DrawLine(d, a);
    }
#endif
}
