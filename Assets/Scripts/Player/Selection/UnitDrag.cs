//using System.Collections;
//using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitDrag : MonoBehaviour
{
    Camera myCam;

    //Graphical
    [SerializeField]
    RectTransform boxVisual;

    //logical
    Rect selectionBox;

    Vector2 startPosition;
    Vector2 endPosition;

    // Start is called before the first frame update
    void Start()
    {
        myCam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    private void OnEnable()
    {
        /*
        EventManager.Player.onMouseLeftClickPressed += MousePressed;
        EventManager.Player.onMouseLeftClickHeld += MouseHeld;
        EventManager.Player.onMouseLeftClickReleased += MouseReleased;
        */
    }

    private void OnDisable()
    {
        /*
        EventManager.Player.onMouseLeftClickPressed -= MousePressed;
        EventManager.Player.onMouseLeftClickHeld -= MouseHeld;
        EventManager.Player.onMouseLeftClickReleased -= MouseReleased;
        */
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0)) //When clicked
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect();
        }

        if (Input.GetMouseButton(0)) //When dragged
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }
        
        if (Input.GetMouseButtonUp(0)) // When released
        {
            SelectUnits();
            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
        
    }

    void MousePressed()
    {
        startPosition = Input.mousePosition;
        selectionBox = new Rect();
    }

    void MouseHeld()
    {
        endPosition = Input.mousePosition;
        DrawVisual();
        DrawSelection();
    }

    void MouseReleased()
    {
        SelectUnits();
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    void DrawVisual()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        Vector2 boxCenter = (boxStart + boxEnd)/2;
        boxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x-boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection()
    {
        if (Input.mousePosition.x < startPosition.x)
        {
            //draggin left
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            //dragging right
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        //y calc
        if (Input.mousePosition.y < startPosition.y)
        {
            //dragging down
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }
    void SelectUnits()
    {
        //loop all units
        foreach (var unit in UnitSelections.Instance.unitList)
        {
            //check if they are inside the box
            if(selectionBox.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelections.Instance.DragSelect(unit);
            }
        }

        //Overlay.Instance.ShowUnitSelections();
    }

}
