using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHp = 100;
    public int hp;

    public event Action<float> OnHealthChanged;
    public event Action<int> OnDamaged; // damage amount
    public event Action OnDied;

    private bool _dead;

    void Awake()
    {
        hp = maxHp;
        _dead = (hp <= 0);
        Notify();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (_dead) return;

        hp = Mathf.Max(hp - amount, 0);
        OnDamaged?.Invoke(amount);
        Notify();

        if (hp == 0)
        {
            _dead = true;
            OnDied?.Invoke();
        }
    }

    void Notify()
    {
        OnHealthChanged?.Invoke((float)hp / maxHp);
    }
}
