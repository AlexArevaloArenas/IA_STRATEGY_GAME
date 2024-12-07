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

    public float viewRange;
    public float MaxHealth;
    public float currentHealth;
    public float AttackDamage;
    public float AttackRange;
    public float MoveRange;

    public string team;

    private void Start()
    {
        pathfindingAgent = GetComponent<Agent>();
        viewRange = MoveRange * 3;
    }
    
    private void Update(){

        
        if(team == "Enemy")
        {
            if(GameManager.Instance.visibleAlivePlayerTeam.Count < 5){

                for(int i = 0; i < GameManager.Instance.playerTeam.Count; i++){

                    Unit unit = GameManager.Instance.playerTeam[i];
                    Debug.Log("Unit: " + unit);
                    if(!unit.visible && Vector3.Distance(transform.position, unit.transform.position) <= viewRange){
                        Debug.Log("Te ha pillao " + gameObject.name);
                        Debug.Log("Unit pillada: " + unit);
                        unit.visible = true;
                        GameManager.Instance.visibleAlivePlayerTeam.Add(unit);
                    }
                }
            }
        }
        
    }

    //HEALTHBARS
    [SerializeField] private Image hpBar;
    [SerializeField] public Sprite picture;
    [SerializeField] public string unitName;
    //hpBar.fillAmount = health / maxHealth;

    public void Move(Vector3 punto)
    {
        if (pathfindingAgent.IsPlaceAvailable(punto,AttackRange))
        {
            pathfindingAgent.GoTo(punto);
            GameManager.Instance.EndUnitAction();
        }   
    }

    public void Attack(Unit enemy)
    {
        Debug.Log("Voy a intentar atacar!!");
        //Vector3 direction = (enemy.transform.position - transform.position ).normalized;
        //LayerMask.NameToLayer("Unit"))
        //Physics.Raycast(transform.position, direction, AttackRange, LayerMask.NameToLayer("Unit"))
        if (Vector3.Distance(transform.position,enemy.transform.position) < AttackRange && currentHealth>0)
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
            GameManager.Instance.TeamCheck();
            GameManager.Instance.EndUnitAction();
        }
    }

    public void GetDamage(float damage)
    {
        Debug.Log("Me atacan!!");
        currentHealth -= damage;
        if (currentHealth < 0) {
            Die();
        }
    }

    public void Revive()
    {
        currentHealth = MaxHealth / 2;
        if (team == "Player")
        {
            transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = Color.magenta;
        }
        
        GetComponent<Unit>().enabled = true;
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    private void Die()
    {
        transform.GetChild(1).gameObject.GetComponent<Renderer>().material.color = Color.grey;
        GetComponent<Unit>().enabled = false;
    }


   


}

public enum UnitType
{
    Knight,
    Mage,
    Archer,
    Pawn
}


