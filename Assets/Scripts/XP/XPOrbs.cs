using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("XP")]
    public int amount = 1;

    [Header("Magnet")]
    public float magnetSpeed = 12f;
    public float magnetAccel = 35f;
    public float maxMagnetSpeed = 25f;

    Transform _target;
    Vector3 _vel;

    public void SetAmount(int v) => amount = v;

    // Called by a magnet trigger on the ship (recommended)
    public void StartMagnet(Transform target)
    {
        _target = target;
    }

    void Update()
    {
        if (_target == null) return;

        // smooth accel towards target (feels better than MoveTowards)
        Vector3 toTarget = (_target.position - transform.position);
        Vector3 dir = toTarget.normalized;

        _vel += dir * (magnetAccel * Time.deltaTime);
        _vel = Vector3.ClampMagnitude(_vel, maxMagnetSpeed);

        transform.position += _vel * Time.deltaTime;

        // optional: little spin
        transform.Rotate(0f, 180f * Time.deltaTime, 0f, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ship should have PlayerXP somewhere in parent hierarchy
        var xp = other.GetComponentInParent<PlayerXP>();
        if (xp == null) return;

        int gained = amount;

        // XP gain upgrade lives on the ship/player
        var upgrades = other.GetComponentInParent<PlayerUpgrades>();
        if (upgrades != null && upgrades.xpGainPct != 0f)
        {
            float mult = 1f + upgrades.xpGainPct / 100f;
            gained = Mathf.Max(1, Mathf.RoundToInt(amount * mult));
        }

        xp.AddXP(gained);
        Destroy(gameObject);
    }
}
