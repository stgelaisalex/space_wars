using UnityEngine;

public class DummyOrbit : MonoBehaviour
{
    public float radius = 2f;
    public float speed = 1.5f;

    Vector3 center;
    float angle;

    void Start()
    {
        center = transform.position;
    }

    void Update()
    {
        angle += speed * Time.deltaTime;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = center + new Vector3(x, 0f, z);
    }
}
