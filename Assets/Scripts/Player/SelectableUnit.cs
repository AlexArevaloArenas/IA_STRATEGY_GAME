//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class SelectableUnit : MonoBehaviour
{
    
    private bool selected = false;

    void Start()
    {
        UnitSelections.Instance.unitList.Add(this.gameObject);
    }
    void OnDestroy()
    {
        UnitSelections.Instance.unitList.Remove(this.gameObject);
    }
    public void Selected(bool s){
        selected = s;
    }
    public bool GetSelected() {
        return selected;
    }

    

}
