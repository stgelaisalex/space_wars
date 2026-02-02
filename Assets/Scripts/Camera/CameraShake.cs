using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float defaultDuration = 0.06f;
    public float defaultStrength = 0.05f;

    float t;
    float duration;
    float strength;

    public Vector3 Offset { get; private set; }

    void LateUpdate()
    {
        if (t <= 0f)
        {
            Offset = Vector3.zero;
            return;
        }

        t -= Time.deltaTime;

        float n = Mathf.Clamp01(t / duration); // 1 -> 0
        float s = strength * n;

        Vector2 r = Random.insideUnitCircle * s;
        Offset = new Vector3(r.x, r.y, 0f);
    }

    public void Kick(float strengthScale = 1f, float durationScale = 1f)
    {
        duration = defaultDuration * durationScale;
        strength = defaultStrength * strengthScale;
        t = duration;
    }
}
