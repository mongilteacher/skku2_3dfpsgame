using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class Player : MonoBehaviour
{
    private PlayerStats _stats;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    public bool TryTakeDamage(float damage)
    {
        if (_stats.Health.Value <= 0) return false;
        
        _stats.Health.Consume(damage);

        return true;
    }
}
