using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    private float speed = 5.0f;
    Vector2 startPos;

    void Start()
    {
        startPos = transform.position;                                      //Store the starting x,y values of the object, set in transform 
    }
    void Update()
    {
        float newPos = Mathf.Repeat(Time.time * speed, 20);              //Repeat the movement
        transform.position = startPos + Vector2.down * newPos;          //Move background from startposition, downwards by the new position
    }
}
