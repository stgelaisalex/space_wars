using UnityEngine;
using System.Collections;

public class ShipHealthUI : MonoBehaviour
{
    public HealthHUD hud; // drag UIDocument object (with HealthHUD)
    public float flashDuration = 0.10f;
    public CameraShake cameraShake;
    [Range(1f, 2.5f)] public float flashBoost = 1.6f;

    Health health;
    Coroutine flashCo;

    void Start()
    {
        health = GetComponent<Health>();

        if (hud == null || hud.bar == null)
        {
            Debug.LogError("HUD or HUD.bar missing.");
            return;
        }

        if (cameraShake == null && Camera.main != null)
            cameraShake = Camera.main.GetComponent<CameraShake>();

        if (cameraShake == null)
            Debug.LogWarning("No CameraShake found on Main Camera.");

        hud.bar.SetValue((float)health.hp / health.maxHp);

        health.OnHealthChanged += hud.bar.SetValue;
        health.OnDamaged += HandleDamaged;
    }

    void OnDestroy()
    {
        if (health != null)
        {
            if (hud != null && hud.bar != null)
                health.OnHealthChanged -= hud.bar.SetValue;

            health.OnDamaged -= HandleDamaged;
        }
    }

    void HandleDamaged(int dmg)
    {
        cameraShake?.Kick(0.1f);

        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(FlashBar());
    }

    IEnumerator FlashBar()
    {
        var bar = hud.bar;

        // store original colors
        Color fill = bar.fillColor;
        Color cap = bar.capColor;
        Color frame = bar.frameColor;
        Color bg = bar.backgroundColor;

        // boosted (flash) colors
        Color Boost(Color c) => new Color(
            Mathf.Clamp01(c.r * flashBoost),
            Mathf.Clamp01(c.g * flashBoost),
            Mathf.Clamp01(c.b * flashBoost),
            c.a
        );

        bar.SetTheme(Boost(fill), Boost(cap), Boost(frame), bg);

        yield return new WaitForSeconds(flashDuration);

        bar.SetTheme(fill, cap, frame, bg);
        flashCo = null;
    }
}
