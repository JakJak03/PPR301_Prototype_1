using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestKnockback : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField]
    float knockForce = 10f;
    [SerializeField]
    Vector2 direction;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        knockForce = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            KnockBack.Begin(rb, direction, knockForce);
        }
    }
}
