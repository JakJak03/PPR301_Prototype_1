using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviourScript : MonoBehaviour
{
    private Rigidbody2D enemyRb;
    [SerializeField]
    private float moveSpeed;

    [SerializeField] private float attackDelay;

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        //enemyRb.velocity = new Vector3(moveSpeed * , playerRb.velocity.y, 0);
    }
}
