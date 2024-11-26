using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SelectableUnit))]
[RequireComponent(typeof(Agent))]
public class Unit : MonoBehaviour
{
    public bool visible;
    public UnitType type;
    private Agent pathfindingAgent;

    public float MaxHealth;
    private float currentHealth;
    public float AttackDamage;
    public float AttackRange;
    public float MoveRange;

    private void Start()
    {
        pathfindingAgent = GetComponent<Agent>();
    }

    //HEALTHBARS
    [SerializeField] private Image hpBar;
    [SerializeField] public Sprite picture;
    [SerializeField] public string unitName;
    //hpBar.fillAmount = health / maxHealth;

    public void Move(Vector3 punto)
    {
        pathfindingAgent.GoTo(punto);
    }

    public void Attack(GameObject enemy)
    {
        return;
    }

    public void GetDamage()
    {

    }

}

public enum UnitType
{
    Knight,
    Mage,
    Archer,
    Pawn
}
