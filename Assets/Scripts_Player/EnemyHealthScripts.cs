using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthScripts : MonoBehaviour
{
    [SerializeField] private int currentHealth;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerSword"))
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        currentHealth--;
        if(currentHealth <= 0 )
        {
            Destroy(gameObject);
        }
    }
}
