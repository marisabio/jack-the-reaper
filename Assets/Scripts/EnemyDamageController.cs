using System;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageController : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float deathTime;
    public UnityEvent onTakeDamage;
    private float _currentHealth;
    
    void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        
        onTakeDamage.Invoke();
        
        if (_currentHealth <= 0)
        {
            Invoke(nameof(Die), deathTime);
        }
    }
    private void Die()
    {
        Destroy(gameObject);
    }
    
}
