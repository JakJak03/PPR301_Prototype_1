using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementScript : MonoBehaviour
{
    [SerializeField] private float attackDistance;
    private float currentDistance;
    [SerializeField] private float moveSpeed;
    private Rigidbody2D myRb;

    private void Start()
    {
        myRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        currentDistance = Vector2.Distance(FindAnyObjectByType<PlayerCombatScript>().transform.position, transform.position);
        if(currentDistance > attackDistance)
            myRb.velocity = new Vector3(moveSpeed * -1, myRb.velocity.y, 0);
    }
}
