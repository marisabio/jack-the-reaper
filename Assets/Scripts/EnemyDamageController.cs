using System;
using UnityEngine;
using UnityEngine.Events;

public class EnemyDamageController : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float deathTime;
    public UnityEvent onTakeDamage;
    private float currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        onTakeDamage.Invoke();
        
        if (currentHealth <= 0)
        {
            Invoke(nameof(Die), deathTime);
        }
    }
    private void Die()
    {
        Destroy(gameObject);
    }
    
}
