using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDXPBinder : MonoBehaviour
{
    [SerializeField] PlayerXP player;

    Label levelLabel;
    XPBar xpBar;

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        levelLabel = root.Q<Label>("levelLabel");
        xpBar = root.Q<XPBar>("xpBar");
        Debug.Log($"HUDXPBinder: player={(player ? player.name : "NULL")} levelLabel={(levelLabel!=null)} xpBar={(xpBar!=null)}");

        if (player != null)
            player.Changed += Refresh;

        Refresh();
    }

    void OnDisable()
    {
        if (player != null)
            player.Changed -= Refresh;
    }

    void Refresh()
    {
        Debug.Log($"Refresh: Level={player.Level} progress={player.Progress01()}");
        if (player == null) return;

        if (levelLabel != null)
            levelLabel.text = $"LEVEL {player.Level}";

        if (xpBar != null)
            xpBar.value = player.Progress01();
    }
}
