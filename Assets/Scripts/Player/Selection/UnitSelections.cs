//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitSelected= new List<GameObject>();

    private static UnitSelections _instance;
    public static UnitSelections Instance {  get { return _instance; } }

    void Awake()
    {
        //If an instance of this already exists and it isn't this one
        if(_instance != null && _instance != this)
        {
            //We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            //Make this the instance
            _instance = this;
        }
    }

    public void ClickSelect(GameObject model)
    {
        GameObject unitToAdd = model.transform.parent.gameObject;
        DeselectAll();
        unitSelected.Add(unitToAdd);
        unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
        //unitToAdd.GetComponent<UnitMovement>().enabled = true;

        unitToAdd.GetComponent<SelectableUnit>().Selected(true);

        //Overlay
        //Overlay.Instance.ShowOverlay();
        //Overlay.Instance.AddUnitInfo(unitToAdd.GetComponent<Unit>());
    }
    public void ShiftClickSelect(GameObject model)
    {
        GameObject unitToAdd = model.transform.parent.gameObject;
        if (!unitSelected.Contains(unitToAdd))
        {
            unitSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);

            unitToAdd.GetComponent<SelectableUnit>().Selected(true);


        }
        else
        {
            unitToAdd.transform.GetChild(0).gameObject.SetActive(false);
            unitSelected.Remove(unitToAdd);
        }
    }
    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitSelected.Contains(unitToAdd))
        {
            unitSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
            //unitToAdd.GetComponent<UnitMovement>().enabled = true;
            unitToAdd.GetComponent<SelectableUnit>().Selected(true);
        }
    }
    public void DeselectAll()
    {
        foreach (var unit in unitSelected)
        {
            //unit.GetComponent<UnitMovement>().enabled = false;
            unit.GetComponent<SelectableUnit>().Selected(false);
            unit.transform.GetChild(0).gameObject.SetActive(false);
        }
        unitSelected.Clear();
            //Overlay.Instance.HideOverlay();
        //Overlay.Instance.HideUnitSelections();
    }
    public void Deselect(GameObject unitToDeselect) 
    {

    }

    public void FirstSelectedUnit()
    {

    }

    public void AddSelection()
    {
        //EventManager.HUD.reloadOverlayUnits.Invoke();
    }

}
