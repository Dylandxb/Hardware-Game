using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{


    private float baseSpeed = 10.0f;
    public Rigidbody2D rb;
    private Vector2 directionDown = Vector2.down;
    
    void Update()
    {
        rb.MovePosition(rb.position + (directionDown* baseSpeed * Time.deltaTime));
    }
}
