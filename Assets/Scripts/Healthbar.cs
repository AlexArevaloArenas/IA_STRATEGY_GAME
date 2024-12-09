using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Unit unidad;
    public GameObject barraVerde;
    Vector3 nuevaPos;
    [SerializeField] Vector2 margen = new Vector2(0,0);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        nuevaPos = new Vector3(unidad.transform.position.x, unidad.transform.position.y+margen.x, unidad.transform.position.z+margen.y);
        transform.position = nuevaPos;
        barraVerde.GetComponent<Image>().fillAmount = CalculaFill(unidad);
    }

    float CalculaFill(Unit u){
        return u.currentHealth/u.MaxHealth;
    }

    
}
