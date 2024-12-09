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

        if (Input.GetMouseButtonUp(1) && selections.unitSelected.Count >0 && selections.unitSelected[0].GetComponent<Unit>().currentHealth>0 && GameManager.Instance.isPlayerTurn==true) // When released
        {
            //Debug.Log("Pepe Viyuela");
            RaycastHit hit;
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                //Debug.Log(hit.collider.gameObject);
                //Debug.Log("Pepe Castañuela 1");
                if (hit.collider.gameObject.layer== LayerMask.NameToLayer("Ground"))
                {
                    //Debug.Log("Pepe Castañuela 2");
                    selections.unitSelected[0].GetComponent<Unit>().Move(new Vector3(hit.point.x, hit.point.y, hit.point.z));
                    
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Unit"))
                {
                    
                    if(hit.collider.gameObject.transform.parent.GetComponent<Unit>().currentHealth <= 0 && GameManager.Instance.PlayerBlood>0)
                    {
                        Debug.Log(hit.collider.gameObject.transform.parent.GetComponent<Unit>().team);
                        GameManager.Instance.PlayerBlood = GameManager.Instance.PlayerBlood - 1;
                        hit.collider.gameObject.transform.parent.GetComponent<Unit>().currentHealth = selections.unitSelected[0].GetComponent<Unit>().MaxHealth;
                        hit.collider.gameObject.transform.parent.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
                        GameManager.Instance.TeamCheck();
                        GameManager.Instance.EndUnitAction();
                    }
                    
                }
                else if(hit.collider.gameObject.layer == LayerMask.NameToLayer("UnitEnemy") && hit.collider.gameObject.transform.parent.GetComponent<Unit>().currentHealth > 0)
                {
                    Debug.Log("Pepe Atacuela");
                    selections.unitSelected[0].GetComponent<Unit>().Attack(hit.collider.gameObject.transform.parent.GetComponent<Unit>());
                }
            }
            
        }
    }
}
