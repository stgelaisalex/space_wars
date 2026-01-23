using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Quick debug overlay to verify upgrades are being applied in-game.
/// Toggle with F1 (new Input System).
/// Uses OnGUI for speed (no UXML needed).
/// </summary>
public class UpgradeDebugOverlay : MonoBehaviour
{
    [Header("Refs (optional)")]
    [SerializeField] PlayerUpgrades upgrades;
    [SerializeField] ShipController ship;          // optional, for showing live movement params
    [SerializeField] ShipWeapon weapon;            // optional, for showing live weapon params
    [SerializeField] Health health;                // optional, for showing live hp

    [Header("Toggle")]
    [SerializeField] Key toggleKey = Key.F1;

    [Header("Display")]
    [SerializeField] bool startVisible = false;

    bool visible;

    void Awake()
    {
        visible = startVisible;

        if (upgrades == null)
            upgrades = FindFirstObjectByType<PlayerUpgrades>();

        if (ship == null)
            ship = FindFirstObjectByType<ShipController>();

        if (weapon == null)
            weapon = FindFirstObjectByType<ShipWeapon>();

        if (health == null)
            health = FindFirstObjectByType<Health>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
            visible = !visible;
    }

    void OnGUI()
    {
        if (!visible) return;

        const float w = 420f;
        const float pad = 12f;
        Rect r = new Rect(pad, pad, w, Screen.height - pad * 2f);

        GUILayout.BeginArea(r, GUI.skin.box);
        GUILayout.Label("<b>UPGRADE DEBUG</b>  (F1 to toggle)", RichLabel());

        GUILayout.Space(6);
        GUILayout.Label($"Time.timeScale: {Time.timeScale:0.###}");

        if (upgrades == null)
        {
            GUILayout.Space(6);
            GUILayout.Label("PlayerUpgrades: <color=yellow>NOT FOUND</color>", RichLabel());
            GUILayout.EndArea();
            return;
        }

        GUILayout.Space(8);
        GUILayout.Label("<b>Raw Upgrade Values</b>", RichLabel());
        Row("Move Speed %", upgrades.moveSpeedPct);
        Row("Attack Speed %", upgrades.attackSpeedPct);
        Row("XP Gain %", upgrades.xpGainPct);
        Row("Magnet Radius %", upgrades.magnetRadiusPct);
        Row("Dodge Chance %", upgrades.dodgeChancePct);
        Row("Damage +", upgrades.damagePct, isPercent:false);
        Row("Health %", upgrades.healthBonusPct);

        GUILayout.Space(8);
        GUILayout.Label("<b>Derived Multipliers</b>", RichLabel());
        Row("Move Speed x", 1f + upgrades.moveSpeedPct / 100f, isPercent:false, format:"0.###");
        Row("Attack Speed x", 1f + upgrades.attackSpeedPct / 100f, isPercent:false, format:"0.###");
        Row("XP Gain x", 1f + upgrades.xpGainPct / 100f, isPercent:false, format:"0.###");
        Row("Magnet Radius x", 1f + upgrades.magnetRadiusPct / 100f, isPercent:false, format:"0.###");
        Row("Dodge Chance", Mathf.Clamp01(upgrades.dodgeChancePct / 100f), isPercent:false, format:"0.### (0..1)");

        // Optional live reads from systems (if you expose them later)
        GUILayout.Space(8);
        GUILayout.Label("<b>Live Systems</b>", RichLabel());

        if (ship != null)
            GUILayout.Label($"ShipController: OK", RichLabel());
        else
            GUILayout.Label("ShipController: (not assigned)", RichLabel());

        if (weapon != null)
            GUILayout.Label($"ShipWeapon: OK", RichLabel());
        else
            GUILayout.Label("ShipWeapon: (not assigned)", RichLabel());

        if (health != null)
            GUILayout.Label($"Health: HP {health.hp:0}/{health.maxHp:0}", RichLabel());
        else
            GUILayout.Label("Health: (not assigned)", RichLabel());

        GUILayout.EndArea();
    }

    void Row(string label, float value, bool isPercent = true, string format = "0")
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(160f));
        string v = value.ToString(format);
        if (isPercent) v += "%";
        GUILayout.Label(v);
        GUILayout.EndHorizontal();
    }

    GUIStyle RichLabel()
    {
        var s = new GUIStyle(GUI.skin.label);
        s.richText = true;
        return s;
    }
}
