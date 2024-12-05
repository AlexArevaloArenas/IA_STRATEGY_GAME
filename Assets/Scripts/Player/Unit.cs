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

    public string team;

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
        GameManager.Instance.EndUnitAction();
    }

    public void Attack(Unit enemy)
    {
        Debug.Log("Voy a intentar atacar!!");
        //Vector3 direction = (enemy.transform.position - transform.position ).normalized;
        //LayerMask.NameToLayer("Unit"))
        //Physics.Raycast(transform.position, direction, AttackRange, LayerMask.NameToLayer("Unit"))
        if (Vector3.Distance(transform.position,enemy.transform.position) < AttackRange)
        {
            UnitType enemytype = enemy.GetComponent<Unit>().type;
            Debug.Log("Ataco!!");
            switch (type)
            {
                case UnitType.Mage:
                    float heightMultiplier = 1;
                    if (transform.position.y> enemy.transform.position.y){
                        heightMultiplier = 1.5f;
                    }
                    switch (enemytype)
                    {
                        case UnitType.Mage:
                            enemy.GetDamage(AttackDamage* heightMultiplier);
                            break;
                        case UnitType.Knight:
                            enemy.GetDamage(AttackDamage*2* heightMultiplier);
                            break;
                        case UnitType.Archer:
                            enemy.GetDamage((AttackDamage * heightMultiplier) / 2);
                            break;
                        case UnitType.Pawn:
                            enemy.GetDamage(AttackDamage * heightMultiplier);
                            break;
                        default:
                            break;
                    }
                    break;
                case UnitType.Knight:
                    switch (enemytype)
                    {
                        case UnitType.Mage:
                            
                            enemy.GetDamage((AttackDamage) / 2);
                            break;
                        case UnitType.Knight:
                            enemy.GetDamage(AttackDamage);
                            break;
                        case UnitType.Archer:
                            
                            enemy.GetDamage(AttackDamage * 2);
                            break;
                        case UnitType.Pawn:
                            enemy.GetDamage(AttackDamage);
                            break;
                        default:
                            break;
                    }
                    break;
                case UnitType.Archer:
                    float heightMultiplier3 = 1;
                    if (transform.position.y > enemy.transform.position.y)
                    {
                        heightMultiplier = 1.5f;
                    }
                    switch (enemytype)
                    {
                        case UnitType.Mage:
                            enemy.GetDamage(AttackDamage * 2 * heightMultiplier3);
                            break;
                        case UnitType.Knight:
                            
                            enemy.GetDamage((AttackDamage * heightMultiplier3) / 2);
                            break;
                        case UnitType.Archer:
                            enemy.GetDamage(AttackDamage * heightMultiplier3);
                            
                            break;
                        case UnitType.Pawn:
                            enemy.GetDamage(AttackDamage * heightMultiplier3);
                            break;
                        default:
                            break;
                    }
                    break;
                case UnitType.Pawn:
                    enemy.GetDamage(AttackDamage);
                    break;
                default:
                    break;

            }

            GameManager.Instance.EndUnitAction();
        }
    }

    public void GetDamage(float damage)
    {
        Debug.Log("Me atacan!!");
        currentHealth -= damage;
        if (currentHealth < 0) {
            Destroy(gameObject);
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void OnDestroy()
    {
        //Dead Animation and Particles
    }
}

public enum UnitType
{
    Knight,
    Mage,
    Archer,
    Pawn
}
