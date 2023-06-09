using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GAWController_Sample : MonoBehaviour
{
    private Color colour;

    private int position= 0;
    private string message = "GAME A WEEK 2021  THEME 5  HARDWARE ";
    
    void Start()
    {
        colour = new Color(Random.value, Random.value, Random.value);
    }

    void Awake()
    {
        position = 0;
        StartCoroutine(UpdatePosition());
    }
    
    IEnumerator UpdatePosition()
    {
        while (isActiveAndEnabled)
        {
            yield return new WaitForSeconds(0.8f);
            position += 1;
            if (position >= message.Length)
            {
                position = 0;
            }
        }
    }
    
    void Update()
    {
        GAWController.ClearPixels();

        if (GAWController.GetButtonUp(GAWController.Button.ButtonOne))
        {
            
        }

        if (GAWController.GetButtonDown(GAWController.Button.ButtonTwo))
        {
            colour = new Color(Random.value, Random.value, Random.value);
        }
        if (GAWController.IsDown(GAWController.Button.ButtonOne))
        {
            //Change direction state to Right
            ArrowRight();
        }
        else if (GAWController.IsDown(GAWController.Button.ButtonTwo))
        {
            //Change direction state to Left
            ArrowLeft();
        }


        else
        {
          
            //StopSignLED();
        }
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




    void StopSignLED()
    {
        GAWController.SetAllPixels(Color.red);
    }

    void GoSignLED()
    {
        GAWController.SetAllPixels(Color.green);
    }
}
