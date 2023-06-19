using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D playerRb;
    [SerializeField]
    private float playerSpeed;

    private bool canMove;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();

        canMove = true;
    }
    private void FixedUpdate()
    {
        if (!canMove)
            return;

        playerRb.velocity = new Vector3(playerSpeed * Input.GetAxis("Horizontal"), playerRb.velocity.y, 0);
    }
}
