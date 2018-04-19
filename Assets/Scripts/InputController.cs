using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input controller - manages all input events for the game
/// </summary>
public class InputController : SingletonMonoBehaviour<InputController>
{


    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 lastTouchPosition = Vector2.zero;
    private float touchAngle = 0;
    private bool isDragging = false;

    private List<Dot> currentDotPath = new List<Dot>();

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            OnFinishedTouch();
        }
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }
        if (isDragging)
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast(mouseRay.origin, mouseRay.direction, 100);
            if (hit2D.collider)
            {
                var dot = hit2D.transform.GetComponent<Dot>();
                //Debug.Log("Hit dot " + dot.name);
                OnDotTouched(dot);
            }
        }
    }

    private void OnFinishedTouch()
    {
        isDragging = false;
        BoardController.Instance.FinalizeDotSelection(currentDotPath);
        // clear selection
        currentDotPath.Clear();
    }

    public void OnDotTouched(Dot dot)
    {
        //Debug.Log("new dot touched : " + dot.name);
        BoardController.Instance.UpdateDotSelection(dot, currentDotPath);
    }

}
