using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CarControl : MonoBehaviour
{
    private Color colour;
    private float baseSpeed = 5.0f;
    public Rigidbody2D rb;
    private Vector2 directionState = Vector2.up;
    private Vector2 directionLeft = new Vector2(-98, 0);
    private Vector2 directionRight = new Vector2(98, 0);
    private Transform transformMove;
    bool isLeft = false;
    bool isRight = false;
    bool isMiddle = false;
    bool isMoving = false;
    bool laneSwitch = false;
    AudioSource audioSrc;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transformMove = GetComponent<Transform>();
        audioSrc = GetComponent<AudioSource>();
    }

    void Update()
    {
        CarMove();

        
        if (GAWController.GetButtonDown(GAWController.Button.ButtonOne) && rb.transform.position.x >= -31.64)
        {
            rb.MovePosition(rb.position + (directionLeft * baseSpeed * Time.deltaTime));                            //Shift the car left by -98 on the x plane
            isLeft = true;                                                                                              //Set isLeft and laneSwitch to true 
            laneSwitch = true;
        }
        else if (GAWController.GetButtonDown(GAWController.Button.ButtonTwo) && rb.transform.position.x <= 30.4)
        {
            
            rb.MovePosition(rb.position + (directionRight * baseSpeed * Time.deltaTime));                   //Check if cars position is at a given vector and prevent a move right, restrict keep object moving in same x plane
            isRight = true;
            laneSwitch = true;
        }
        else if (rb.transform.position.x >= -3.09 && rb.transform.position.x <= 1.94)
        {
            isMiddle = true;
            
        }
        if (isMoving == true)                                                                                //Play car audio source whilst it is moving
        {
            if (!audioSrc.isPlaying)
            {
                audioSrc.Play();
            }
        }
        else
        {
            audioSrc.Stop();
        }

        if (isLeft == true && rb.transform.position.x >= -31.64 && rb.transform.position.x <= -3.09)            //Check the position of the vector within the screen to determine which lane LED lights up
        {
            LeftLaneLightUp();
        }
        else if(isRight == true && rb.transform.position.x >= 1.94 && rb.transform.position.x <= 30.4)          //If the car isRight and within the right lane boundary then it calls the LED function RightLaneLightUp
        {
            RightLaneLightUp();
        }
        else if (isMiddle == true)
        {
            MiddleLaneLightUp();
        }
        else if (laneSwitch == true)
        {
            GAWController.ClearPixels();                                                                    //Reset the current LED pixels once a car has changed lane, to show the new LED colourway after a lane change
        }
        else
        {
            AllLanesLightUp();
        }

        
    }

    
    private void CarMove()
    {
        rb.MovePosition(rb.position + (directionState * baseSpeed * Time.deltaTime));               //Constantly Move rigidbody from its current position "up" by a speed * small amount of time
        isMoving = true;
        MiddleLaneLightUp();
    }

    void ArrowUp()
    {
        GAWController.DrawLine(4, 0, 4, 7, Color.green);
        GAWController.DrawLine(3, 0, 3, 7, Color.green);
        GAWController.DrawLine(2, 7, 2, 6, Color.green);
        GAWController.DrawLine(5, 7, 5, 6, Color.green);
        GAWController.DrawLine(1, 6, 1, 5, Color.green);
        GAWController.DrawLine(2, 5, 2, 5, Color.green);
        GAWController.DrawLine(5, 6, 5, 5, Color.green);
        GAWController.DrawLine(6, 6, 6, 4, Color.green);
        GAWController.DrawLine(0, 5, 0, 3, Color.green);
        GAWController.DrawLine(1, 6, 1, 4, Color.green);
        GAWController.DrawLine(7, 5, 7, 3, Color.green);
    }

    void ArrowRight()
    {
        GAWController.DrawLine(0, 4, 7, 4, Color.green);
        GAWController.DrawLine(0, 3, 7, 3, Color.green);
        GAWController.DrawLine(0, 4, 7, 4, Color.green);
        GAWController.DrawLine(5, 2, 7, 2, Color.green);
        GAWController.DrawLine(4, 1, 6, 1, Color.green);
        GAWController.DrawLine(3, 0, 5, 0, Color.green);
        GAWController.DrawLine(5, 5, 7, 5, Color.green);
        GAWController.DrawLine(4, 6, 6, 6, Color.green);
        GAWController.DrawLine(3, 7, 5, 7, Color.green);
    }

    void ArrowLeft()
    {
        GAWController.DrawLine(0, 4, 7, 4, Color.green);
        GAWController.DrawLine(0, 3, 7, 3, Color.green);
        GAWController.DrawLine(0, 2, 2, 2, Color.green);
        GAWController.DrawLine(0, 5, 2, 5, Color.green);
        GAWController.DrawLine(1, 1, 3, 1, Color.green);
        GAWController.DrawLine(2, 0, 4, 0, Color.green);
        GAWController.DrawLine(0, 5, 2, 5, Color.green);
        GAWController.DrawLine(1, 6, 3, 6, Color.green);
        GAWController.DrawLine(2, 7, 4, 7, Color.green);
    }

    void AllLanesLightUp()                                                     
    {
        GAWController.DrawLine(0,0, 0, 7, Color.green);
        GAWController.DrawLine(1, 0, 1, 7, Color.green);
        GAWController.DrawLine(3, 0, 3, 7, Color.green);
        GAWController.DrawLine(4, 0, 4, 7, Color.green);
        GAWController.DrawLine(6, 0, 6, 7, Color.green);
        GAWController.DrawLine(7, 0, 7, 7, Color.green);
    }

    void LeftLaneLightUp()
    {
        GAWController.DrawLine(0, 0, 0, 7, Color.green);
        GAWController.DrawLine(1, 0, 1, 7, Color.green);
        GAWController.DrawLine(3, 0, 3, 7, Color.red);
        GAWController.DrawLine(4, 0, 4, 7, Color.red);
        GAWController.DrawLine(6, 0, 6, 7, Color.red);
        GAWController.DrawLine(7, 0, 7, 7, Color.red);
    }

    void MiddleLaneLightUp()
    {
        GAWController.DrawLine(3, 0, 3, 7, Color.green);
        GAWController.DrawLine(4, 0, 4, 7, Color.green);
        GAWController.DrawLine(0, 0, 0, 7, Color.red);
        GAWController.DrawLine(1, 0, 1, 7, Color.red);
        GAWController.DrawLine(6, 0, 6, 7, Color.red);
        GAWController.DrawLine(7, 0, 7, 7, Color.red);
    }

    void RightLaneLightUp()
    {
        GAWController.DrawLine(6, 0, 6, 7, Color.green);
        GAWController.DrawLine(7, 0, 7, 7, Color.green);
        GAWController.DrawLine(3, 0, 3, 7, Color.red);
        GAWController.DrawLine(4, 0, 4, 7, Color.red);
        GAWController.DrawLine(0, 0, 0, 7, Color.red);
        GAWController.DrawLine(1, 0, 1, 7, Color.red);
    }
}
