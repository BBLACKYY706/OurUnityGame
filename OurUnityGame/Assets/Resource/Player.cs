using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    public float speed = 1.0f;
    public float jumpSpeed = 1.0f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
            rb.velocity = new Vector2(Input.GetAxis("Horizontal"), 1);
        else
            rb.velocity = new Vector2(Input.GetAxis("Horizontal"), 0);
    }
}
