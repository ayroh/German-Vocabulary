using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Signals;

public class InputManager : MonoBehaviour
{

    [SerializeField] private int inputThreshold = 200;

    private Vector2 inputStartPoint;
    private bool canSlide = true;
    
    void Update()
    {
        if (Input.touchCount > 1)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            inputStartPoint = Input.mousePosition;
            canSlide = true;
        }
        else if (canSlide && Input.GetMouseButton(0))
        {
            Vector2 input = Input.mousePosition;
            float xDiff = input.x - inputStartPoint.x;
            float yDiff = input.y - inputStartPoint.y;
            if (xDiff > inputThreshold)
            {
                canSlide = false;
                Signals.OnSetNewAnswer?.Invoke(false);
            }
            else if(xDiff < -inputThreshold)
            {
                canSlide = false;
                Signals.OnSetNewAnswer?.Invoke(true);
            }
            else if(yDiff < -inputThreshold)
            {
                canSlide = false;
                Signals.OnResetAnswer?.Invoke();
            }
            else if (yDiff > inputThreshold)
            {
                canSlide = false;
                PuzzleManager.instance.NextCorrectLetter();
            }
        }
        else if(Input.GetMouseButtonUp(0)) 
        {
            canSlide = false;
        }
    }
}
