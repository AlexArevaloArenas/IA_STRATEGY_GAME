using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;

[RequireComponent(typeof(UnitSelections))]
public class UnitInput : MonoBehaviour
{
    private Camera myCam;

    private UnitSelections selections;
    public LayerMask terrain;
    public LayerMask unit;
  


    private void Start()
    {
        selections = GetComponent<UnitSelections>();
        myCam = Camera.main;
    }

    private void Update()
    {
        /*
        if (Input.GetMouseButtonDown(1)) //When clicked
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect();
        }

        if (Input.GetMouseButton(1)) //When dragged
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        */

        if (Input.GetMouseButtonUp(1) && selections.unitSelected.Count>0 && GameManager.Instance.isPlayerTurn==true) // When released
        {
            Debug.Log("Pepe Viyuela");
            RaycastHit hit;
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.layer== terrain)
                {
                    selections.unitSelected[0].GetComponent<Unit>().Move(new Vector3(hit.point.x, hit.point.y, hit.point.z));
                    Debug.Log("Pepe Castañuela");
                }

                if (hit.collider.gameObject.layer == unit)
                {
                    if (hit.collider.GetComponent<Unit>().team == "Enemy")
                    {
                        selections.unitSelected[0].GetComponent<Unit>().Attack(hit.collider.gameObject.GetComponent<Unit>());
                        Debug.Log("Pepe Atacuela");
                    }
                    
                }
            }
            
        }
    }
}
