using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestKnockback : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField]
    float knockForce = 10f;
    [SerializeField]
    [Range(-1,1)]
    int direction;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        direction = 1;
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
