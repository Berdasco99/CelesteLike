using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JumpTest : MonoBehaviour
{
    Rigidbody2D rb;
    public float jumpPower;

    // Start is called before the first frame update
    void Start()
    {
         rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump")) //Checks wheter you are touching the ground before it allows you to jump
        {
            rb.velocity = new Vector2(jumpPower, jumpPower);
        }
    }
}
