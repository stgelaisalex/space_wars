using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 45f, -20f);
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;

        // Base follow
        Vector3 basePos = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        // Additive shake (tiny offset)
        var shake = GetComponent<CameraShake>();
        Vector3 shakeOffset = shake ? shake.Offset : Vector3.zero;

        transform.position = basePos + shakeOffset;
    }
}
