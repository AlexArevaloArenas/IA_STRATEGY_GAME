//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class UnitClick : MonoBehaviour
{
    private Camera myCam;

    public LayerMask clickable;
    public LayerMask terrain;
    public LayerMask UI;
    public GameObject terrainMark;

    void Start()
    {
        myCam = Camera.main;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, UI))
            {
                print("UI");
            }
            else
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
                {
                    //If we hit a clickable object


                    if (Input.GetKey(KeyCode.LeftShift))
                    {//or shift click
                        UnitSelections.Instance.ShiftClickSelect(hit.collider.gameObject);
                    }
                    else
                    {//Or normal click
                        UnitSelections.Instance.ClickSelect(hit.collider.gameObject);
                    }

                }
                else
                {
                    //We don't and we are not shift clicking
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        UnitSelections.Instance.DeselectAll();

                    }
                }
            }
        }


        if(Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = myCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrain))
            {
                terrainMark.transform.position = new Vector3(hit.point.x, terrainMark.transform.position.y, hit.point.z);
                terrainMark.SetActive(false);
                terrainMark.SetActive(true);
            }
        }
    }
}
