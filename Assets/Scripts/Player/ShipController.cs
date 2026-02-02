using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float thrust = 18f;          // forward accel
    public float strafeThrust = 10f;    // optional, for side drift
    public float maxSpeed = 20f;
    public float linearDamping = 1.5f;  // higher = stops faster

    [Header("Rotation")]
    public float turnSpeed = 180f;      // degrees/sec

    private Vector3 velocity;

    // Upgrades
    PlayerUpgrades upgrades;
    float baseThrust, baseStrafeThrust, baseMaxSpeed;

    void Awake()
    {
        upgrades = GetComponent<PlayerUpgrades>();
        if (upgrades == null) upgrades = GetComponentInParent<PlayerUpgrades>();

        baseThrust = thrust;
        baseStrafeThrust = strafeThrust;
        baseMaxSpeed = maxSpeed;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- Input ---
        float turn = 0f;
        float forward = 0f;
        float strafe = 0f;

        if (Keyboard.current != null)
        {
            turn = (Keyboard.current.aKey.isPressed ? -1f : 0f) +
                   (Keyboard.current.dKey.isPressed ? 1f : 0f);

            forward = (Keyboard.current.wKey.isPressed ? 1f : 0f) +
                      (Keyboard.current.sKey.isPressed ? -1f : 0f);

            // Optional strafing (comment out if you don't want it)
            strafe = (Keyboard.current.qKey.isPressed ? -1f : 0f) +
                     (Keyboard.current.eKey.isPressed ? 1f : 0f);
        }

        // --- Rotate ship ---
        transform.Rotate(0f, turn * turnSpeed * dt, 0f, Space.World);

        // --- Apply move speed upgrades ---
        float speedMult = 1f;
        if (upgrades != null && upgrades.moveSpeedPct != 0f)
            speedMult += upgrades.moveSpeedPct / 100f;

        float curThrust = baseThrust * speedMult;
        float curStrafe = baseStrafeThrust * speedMult;
        float curMaxSpeed = baseMaxSpeed * speedMult;

        // --- Accelerate in ship local axes ---
        Vector3 accel =
            (transform.forward * (forward * curThrust)) +
            (transform.right * (strafe * curStrafe));

        velocity += accel * dt;

        // clamp speed
        float mag = velocity.magnitude;
        if (mag > curMaxSpeed && mag > 0.0001f)
            velocity = (velocity / mag) * curMaxSpeed;

        // damping (spacey glide)
        velocity = Vector3.Lerp(velocity, Vector3.zero, linearDamping * dt);

        // move
        transform.position += velocity * dt;
    }
}
