using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LevelUpOverlayController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PlayerXP player;
    [SerializeField] PlayerUpgrades upgrades;
    [SerializeField] bool pauseOnLevelUp = true;

    [Header("Upgrade Icons")]
    [SerializeField] Texture2D iconMoveSpeed;    // upgrade_speed_icon.png
    [SerializeField] Texture2D iconDodge;        // upgrade_dodge_icon.png
    [SerializeField] Texture2D iconAttackSpeed;  // upgrade_attack-speed_icon.png
    [SerializeField] Texture2D iconXp;           // upgrade_xp_icon.png
    [SerializeField] Texture2D iconMagnet;       // upgrade_magnet_icon.png
    [SerializeField] Texture2D iconDamage;       // upgrade_damage_icon.png
    [SerializeField] Texture2D iconHealth;       // upgrade_shield_icon.png (health/shield)
    [SerializeField] Texture2D iconHealthRegen;       // upgrade_shield_icon.png (health/shield)

    [Header("Rarity Frames (white, blue, red, yellow)")]
    [SerializeField] Texture2D frameWhite;   // upgrade-white.png  (Normal)
    [SerializeField] Texture2D frameBlue;    // upgrade-blue.png   (Rare)
    [SerializeField] Texture2D frameRed;     // upgrade-red.png    (Epic)
    [SerializeField] Texture2D frameYellow;  // upgrade-yellow.png (Legendary)

    VisualElement overlay;
    VisualElement panel;
    Button continueButton;

    // 3 upgrade choice cards (UI Toolkit)
    Button[] choiceButtons;
    VisualElement[] choiceFrames;
    VisualElement[] choiceIcons;
    Label[] choiceTitles;
    Label[] choiceValues;

    // Click handlers (so we can unsubscribe cleanly)
    Action[] choiceClickHandlers;

    bool isOpen;

    // Pause tracking (robust against OnDisable / UI rebuilds)
    bool didPause = false;
    float prevTimeScale = 1f;
    readonly System.Random rng = new System.Random();

    // ---------------- Upgrade model ----------------
    enum UpgradeType
    {
        MoveSpeed,
        DodgeChance,
        AttackSpeed,
        XpGain,
        MagnetRadius,
        Damage,
        Health,
        HealthRegen
    }

    enum Rarity { Normal, Rare, Epic, Legendary }

    struct Offer
    {
        public UpgradeType type;
        public Rarity rarity;
        public float value;     // numeric value
        public bool isPercent;  // display as %
    }

    Offer[] currentOffers = new Offer[3];

    // Values are rarity tiers:
    // Normal, Rare, Epic, Legendary
    // NOTE: Damage is flat, everything else is %
    static readonly Dictionary<UpgradeType, float[]> ValuesByRarity = new()
    {
        { UpgradeType.MoveSpeed,    new[] { 5f, 10f, 15f, 20f } },   // %
        { UpgradeType.DodgeChance,  new[] { 2f, 4f, 6f, 8f } },      // %
        { UpgradeType.AttackSpeed,  new[] { 10f, 15f, 20f, 25f } },  // %
        { UpgradeType.XpGain,       new[] { 8f, 16f, 24f, 32f } },   // %
        { UpgradeType.MagnetRadius, new[] { 10f, 15f, 20f, 25f } },  // %
        { UpgradeType.Damage,       new[] { 9f, 15f, 21f, 28f } },   // FLAT
        { UpgradeType.Health,       new[] { 15f, 20f, 25f, 30f } },  // %
        { UpgradeType.HealthRegen, new[] { 0.5f, 1f, 2f, 3f } }, // HP/sec
    };

    static bool IsPercentUpgrade(UpgradeType type) => type switch
    {
        UpgradeType.HealthRegen => false, // HP per second
        _ => true                          // everything else is %
    };

    // Rarity weights: Normal 50%, Rare 30%, Epic 15%, Legendary 5%
    static Rarity RollRarity(System.Random r)
    {
        int roll = r.Next(0, 100);
        if (roll < 50) return Rarity.Normal;
        if (roll < 80) return Rarity.Rare;
        if (roll < 95) return Rarity.Epic;
        return Rarity.Legendary;
    }

    // Allocation-free type list for 7 upgrades
    static readonly UpgradeType[] AllTypes = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        overlay = root.Q<VisualElement>("levelUpOverlay");
        panel = root.Q<VisualElement>("levelUpPanel");
        continueButton = root.Q<Button>("continueButton");

        if (continueButton != null)
            continueButton.clicked += Close;

        if (player == null)
            player = FindFirstObjectByType<PlayerXP>();

        if (upgrades == null)
        {
            // Prefer upgrades next to PlayerXP (ship)
            if (player != null) upgrades = player.GetComponentInParent<PlayerUpgrades>();
            if (upgrades == null) upgrades = FindFirstObjectByType<PlayerUpgrades>();
        }

        if (player != null)
            player.LeveledUp += Open;

        // ---- Upgrade choice UI wiring ----
        choiceButtons = new Button[3];
        choiceFrames = new VisualElement[3];
        choiceIcons = new VisualElement[3];
        choiceTitles = new Label[3];
        choiceValues = new Label[3];
        choiceClickHandlers = new Action[3];

        for (int i = 0; i < 3; i++)
        {
            choiceButtons[i] = root.Q<Button>($"upgradeChoice{i}");
            if (choiceButtons[i] == null) continue;

            choiceFrames[i] = choiceButtons[i].Q<VisualElement>("frame");
            choiceIcons[i] = choiceButtons[i].Q<VisualElement>("icon");
            choiceTitles[i] = choiceButtons[i].Q<Label>("title");
            choiceValues[i] = choiceButtons[i].Q<Label>("value");

            var btn = choiceButtons[i];

            // Hover in
            btn.RegisterCallback<PointerEnterEvent>(_ =>
            {
                AnimateButtonScale(btn, 1f, 1.05f, 0.08f);
            });

            // Hover out
            btn.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                AnimateButtonScale(btn, 1.05f, 1f, 0.08f);
            });

            // Press
            btn.RegisterCallback<PointerDownEvent>(_ =>
            {
                AnimateButtonScale(btn, 1.05f, 0.96f, 0.04f);
            });

            // Release
            btn.RegisterCallback<PointerUpEvent>(_ =>
            {
                AnimateButtonScale(btn, 0.96f, 1.05f, 0.06f);
            });

            int idx = i; // capture
            choiceClickHandlers[i] = () => OnPickUpgrade(idx);
            choiceButtons[i].clicked += choiceClickHandlers[i];
        }

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

        if (choiceButtons != null && choiceClickHandlers != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null || choiceClickHandlers[i] == null) continue;
                choiceButtons[i].clicked -= choiceClickHandlers[i];
            }
        }

        if (isOpen) Resume();
    }

    void Update()
    {
        // Debug keys (new Input System)
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

        GenerateAndRenderOffers();

        // Start from hidden state, then animate to open
        SetAnimState(open: false, instant: true);

        // Animate UI first, then pause the world
        AnimateIn(onDone: () =>
        {
            if (!pauseOnLevelUp) return;

            // Remember what it was, then pause
            prevTimeScale = Time.timeScale;
            didPause = true;
            Time.timeScale = 0f;
        });
    }


    void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        Resume();      // resumes instantly
        AnimateOut();  // fade out UI using unscaled time
    }

    // ---------------- Upgrade generation + rendering ----------------

    void GenerateAndRenderOffers()
    {
        // Pick 3 distinct types without allocations:
        // make a tiny local copy of indices [0..n-1] and shuffle first 3.
        Span<int> idx = stackalloc int[AllTypes.Length];
        for (int i = 0; i < idx.Length; i++) idx[i] = i;

        // Fisher–Yates partial shuffle for first 3
        for (int i = 0; i < 3; i++)
        {
            int swap = rng.Next(i, idx.Length);
            (idx[i], idx[swap]) = (idx[swap], idx[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            var type = AllTypes[idx[i]];
            var rarity = RollRarity(rng);
            float val = ValuesByRarity[type][(int)rarity];

            currentOffers[i] = new Offer
            {
                type = type,
                rarity = rarity,
                value = val,
                isPercent = IsPercentUpgrade(type)
            };

            RenderOffer(i, currentOffers[i]);
        }
    }

    void RenderOffer(int i, Offer offer)
    {
        if (choiceButtons == null || i < 0 || i >= choiceButtons.Length) return;
        if (choiceButtons[i] == null) return;

        if (choiceTitles[i] != null) choiceTitles[i].text = TitleFor(offer.type);

        if (choiceValues[i] != null)
        {
            if (offer.type == UpgradeType.HealthRegen)
                choiceValues[i].text = $"+{offer.value:0.#} HP/s";
            else
                choiceValues[i].text = offer.isPercent ? $"+{offer.value:0}%" : $"+{offer.value:0}";
        }

        // Frame by rarity: white, blue, red, yellow
        var frameTex = FrameFor(offer.rarity);
        if (choiceFrames[i] != null && frameTex != null)
            choiceFrames[i].style.backgroundImage = new StyleBackground(frameTex);

        // Icon by upgrade type
        var iconTex = IconFor(offer.type);
        if (choiceIcons[i] != null)
        {
            choiceIcons[i].style.backgroundImage =
                iconTex != null ? new StyleBackground(iconTex) : StyleKeyword.None;
        }
    }

    Texture2D FrameFor(Rarity r) => r switch
    {
        Rarity.Normal => frameWhite,
        Rarity.Rare => frameBlue,
        Rarity.Epic => frameRed,
        Rarity.Legendary => frameYellow,
        _ => frameWhite
    };

    Texture2D IconFor(UpgradeType t) => t switch
    {
        UpgradeType.MoveSpeed => iconMoveSpeed,
        UpgradeType.DodgeChance => iconDodge,
        UpgradeType.AttackSpeed => iconAttackSpeed,
        UpgradeType.XpGain => iconXp,
        UpgradeType.MagnetRadius => iconMagnet,
        UpgradeType.Damage => iconDamage,
        UpgradeType.Health => iconHealth,
        UpgradeType.HealthRegen => iconHealthRegen,
        _ => null
    };

    static string TitleFor(UpgradeType t) => t switch
    {
        UpgradeType.MoveSpeed => "Movement Speed",
        UpgradeType.DodgeChance => "Dodge Chance",
        UpgradeType.AttackSpeed => "Attack Speed",
        UpgradeType.XpGain => "XP Gain",
        UpgradeType.MagnetRadius => "Magnet Radius",
        UpgradeType.Damage => "Damage",
        UpgradeType.Health => "Health",
        _ => "Upgrade"
    };

    void OnPickUpgrade(int index)
    {
        if (index < 0 || index >= currentOffers.Length) return;

        var offer = currentOffers[index];

        if (upgrades != null)
            ApplyUpgrade(upgrades, offer);

        Close();
    }

    static void ApplyUpgrade(PlayerUpgrades u, Offer offer)
    {
        switch (offer.type)
        {
            case UpgradeType.MoveSpeed:
                u.moveSpeedPct += offer.value;
                break;
            case UpgradeType.DodgeChance:
                u.dodgeChancePct += offer.value;
                break;
            case UpgradeType.AttackSpeed:
                u.attackSpeedPct += offer.value;
                break;
            case UpgradeType.XpGain:
                u.xpGainPct += offer.value;
                break;
            case UpgradeType.MagnetRadius:
                u.magnetRadiusPct += offer.value;
                break;
            case UpgradeType.Damage:
                u.damagePct += offer.value;
                break;
            case UpgradeType.Health:
                u.healthBonusPct += offer.value;
                break;
            case UpgradeType.HealthRegen:
                u.healthRegenHpPerSec += offer.value;
                break;
        }
    }

    // ---------------- Animation helpers ----------------

    void SetAnimState(bool open, bool instant)
    {
        // Overlay fade
        if (overlay != null)
            overlay.style.opacity = open ? 1f : 0f;

        // Panel pop
        if (panel != null)
        {
            panel.style.opacity = open ? 1f : 0f;
            panel.style.scale = open
                ? new Scale(new Vector2(1f, 1f))
                : new Scale(new Vector2(0.92f, 0.92f));
        }

        if (instant)
        {
            overlay?.MarkDirtyRepaint();
            panel?.MarkDirtyRepaint();
        }
    }

    void AnimateButtonScale(VisualElement el, float from, float to, float duration)
    {
        if (el == null) return;

        float elapsed = 0f;
        IVisualElementScheduledItem item = null;

        item = el.schedule.Execute(() =>
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ease = EaseOutCubic(t);

            float s = Mathf.Lerp(from, to, ease);
            el.style.scale = new Scale(new Vector2(s, s));

            if (t >= 1f)
                item.Pause();
        }).Every(16);
    }

    void AnimateIn(Action onDone = null)
    {
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
        }, onDone);
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
        if (!pauseOnLevelUp) return;
        if (!didPause) return;

        Time.timeScale = prevTimeScale;
        didPause = false;
    }

    void Animate(float duration, Action<float> step, Action onDone = null)
    {
        if (overlay == null) return;

        float elapsed = 0f;
        IVisualElementScheduledItem item = null;

        item = overlay.schedule.Execute(() =>
        {
            elapsed += Time.deltaTime;
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
