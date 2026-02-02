using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDieOnZero : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (health.hp <= 0)
            Destroy(gameObject);
    }
}