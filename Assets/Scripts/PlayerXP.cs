using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public int xp;

    public void AddXP(int amount)
    {
        xp += amount;
        Debug.Log("XP = " + xp);
    }
}
