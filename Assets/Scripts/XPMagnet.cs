using UnityEngine;

public class XPMagnet : MonoBehaviour
{
    [Header("Magnet")]
    public float radius = 3.5f;
    public LayerMask xpLayer; // set to XPOrb
    public float refreshRate = 0.05f; // 20 times/sec

    float _t;

    void Update()
    {
        _t += Time.deltaTime;
        if (_t < refreshRate) return;
        _t = 0f;

        var hits = Physics.OverlapSphere(
            transform.position,
            radius,
            xpLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits.Length; i++)
        {
            var orb = hits[i].GetComponentInParent<XPOrb>();
            if (orb != null)
            {
                orb.StartMagnet(transform);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
