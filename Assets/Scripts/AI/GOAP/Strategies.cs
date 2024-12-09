using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

// TODO Migrate Strategies, Beliefs, Actions and Goals to Scriptable Objects and create Node Editor for them

public interface IActionStrategy
{
    bool CanPerform { get; }
    bool Complete { get; }

    void Start()
    {
        // noop
    }

    void Update(float deltaTime)
    {
        // noop
    }

    void Stop()
    {
        // noop
    }
}

//MOVE TO ENEMY DIRECTION

public class MoveToEnemyStrategy : IActionStrategy
{

    readonly Unit currentUnit;
    readonly Unit targetEnemy;
    bool complete;
    bool moving;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    

    public MoveToEnemyStrategy(Unit currentUnit, Unit targetEnemy)
    {
        this.currentUnit = currentUnit;
        this.targetEnemy = targetEnemy;
        Debug.Log("Dios me ayude");
    }

    public void Start()
    {
        GameManager.Instance.GoapAgent.interacting = true; //Asi no se calculan mas goals mientras se juega

        Debug.Log("MoveToEnemyStrategy");

        
        Agent agent = currentUnit.GetComponent<Agent>();
       

        // First step: Find nearby enemies
        Unit[] nearbyEnemies = agent.EnemiesAvailable();
    
        //POSSIBILITY: Instead of goap agent having a target enemy per default, calculate the target enemy in this step

        GridNode[] bestAttackPlaces = currentUnit.GetComponent<Agent>().FindBestAttackPlaces(
            targetEnemy,
            nearbyEnemies.ToList(),
            currentUnit.type
        );

        if(bestAttackPlaces.Length > 0)
        {
            Debug.Log("HAY SITIO PARA ATACAR");

            float recordDistance = 0f;
            int count = 0;
            if(currentUnit.type == UnitType.Knight || currentUnit.type == UnitType.Pawn)
            {
                recordDistance = Mathf.Infinity;
            }

            for (int i = 0; i < bestAttackPlaces.Length; i++)
            {
                
                float distance = agent.DistanceBetweenTwoNodes(bestAttackPlaces[i].worldPosition, targetEnemy.transform.position);
                Debug.Log("DISTANCIA: " + distance);
                if (currentUnit.type == UnitType.Knight || currentUnit.type == UnitType.Pawn) //Si es caballero o peon, se va al mas cercano
                {
                    if (distance < recordDistance)
                    {
                        recordDistance = distance;
                        count = i;

                    }
                }

                else //En caso contrario, va al mas lejano
                {
                    if (distance > recordDistance)
                    {
                        recordDistance = distance;
                        count = i;

                    }
                }
                
            }


            Vector3 targetPosition = bestAttackPlaces[count].worldPosition;
            agent.GoTo(targetPosition);
            moving = true;
            if (moving)
            {
                moving = false;
                agent.StartCoroutine(WaitUntilReached(agent, targetPosition));
            }
            
        }

        else
        {
            // Second step: Find the best safe places considering all nearby enemies
            GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());
            List<GridNode> safestHeightsPlaces = new List<GridNode>();

            foreach (GridNode n in bestSafePlaces)
            {
                if (n.worldPosition.y > 0)
                {
                    safestHeightsPlaces.Add(n);

                }
            }

            if (safestHeightsPlaces.Count!=0)
            {
                bestSafePlaces = safestHeightsPlaces.ToArray();
            }

            if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
            {
                float minDistance = Mathf.Infinity;
                int count = 0;
                
                for (int i = 0; i < bestSafePlaces.Length; i++)
                {
                    
                    float distance = agent.DistanceBetweenTwoNodes(bestSafePlaces[i].worldPosition, targetEnemy.transform.position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        count = i;

                    }
                }

                Vector3 targetPosition = bestSafePlaces[count].worldPosition;

                agent.GoTo(targetPosition);
                moving = true;
                if (moving)
                {
                    moving = false;
                    agent.StartCoroutine(WaitUntilReached(agent, targetPosition));
                }
               

            }
            

        }

        
    }

    private System.Collections.IEnumerator WaitUntilReached(Agent agent, Vector3 targetPosition)
    {
   
        yield return new WaitForSeconds(1.5f); //Delay timer between moves

        complete = true;
        moving = false;
        Debug.Log("SE HA ACABAO");
        GameManager.Instance.unitsUsed += 1;
        if (GameManager.Instance.unitsUsed >= GameManager.Instance.unitsPerTurn)
        {
            GameManager.Instance.EndAITurn();
        }
        else GameManager.Instance.GoapAgent.TurnStart();



    }

    public void Stop()
    {
        
    }


}

//EXPLORE TO RANDOM ENEMY (CANT ATTACK)

public class ExploreStrategy : IActionStrategy
{

    readonly Unit currentUnit;
    readonly Unit targetEnemy;
    bool complete;
    bool moving;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public ExploreStrategy(Unit currentUnit, Unit targetEnemy)
    {
        this.currentUnit = currentUnit;
        this.targetEnemy = targetEnemy;
    }

    public void Start()
    {
        Debug.Log("EXPLORO FLOROMACOBO");
        GameManager.Instance.GoapAgent.interacting = true; //Asi no se calculan mas goals mientras se juega

        Agent agent = currentUnit.GetComponent<Agent>();


        // First step: Find nearby enemies
        Unit[] nearbyEnemies = agent.EnemiesAvailable();

        // Second step: Find the best safe places considering all nearby enemies
        GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());

        if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
        {
            float minDistance = Mathf.Infinity;
            int count = 0;

            for (int i = 0; i < bestSafePlaces.Length; i++)
            {

                float distance = agent.DistanceBetweenTwoNodes(bestSafePlaces[i].worldPosition, targetEnemy.transform.position);
                //Debug.Log("DISTANCIA: " + distance);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    count = i;

                }
            }

            Vector3 targetPosition = bestSafePlaces[count].worldPosition;

            agent.GoTo(targetPosition);
            moving = true;
            if (moving)
            {
                moving = false;
                agent.StartCoroutine(WaitUntilReached(agent, targetPosition));
            }

        }

    }

    private System.Collections.IEnumerator WaitUntilReached(Agent agent, Vector3 targetPosition)
    {

        yield return new WaitForSeconds(1.5f); //Delay timer between moves

        complete = true;
        moving = false;
        Debug.Log("SE HA ACABAO");
        GameManager.Instance.unitsUsed += 1;
        if (GameManager.Instance.unitsUsed >= GameManager.Instance.unitsPerTurn)
        {
            GameManager.Instance.EndAITurn();
        }
        else GameManager.Instance.GoapAgent.TurnStart();



    }

}

//RESURRECT STRATEGY
public class ResurrectStrategy : IActionStrategy
{

    readonly Unit currentUnit;
    readonly Unit deadAlly;
    readonly GameObject prefab;
    bool complete;

    public bool CanPerform => !complete;
    public bool Complete => complete;



    public ResurrectStrategy(Unit currentUnit, Unit deadAlly, GameObject prefab) //Se le pasa el prefab desed el goapAgent
    {
        this.currentUnit = currentUnit;
        this.deadAlly = deadAlly;
        this.prefab = prefab;
    }



    public void Start()
    {
        GameManager.Instance.GoapAgent.interacting = true; //Asi no se calculan mas goals mientras se juega
        //GameObject.Instantiate(currentUnit, deadAlly.transform.position, Quaternion.identity); //Se instancia la nueva unidad
        //GameManager.Instance.enemyTeam.Add(prefab.GetComponent<Unit>()); //Se agrega la nueva unidad a la lista de enemigos
        deadAlly.Revive();
        GameManager.Instance.AIBlood -= 1;
        Agent agent = currentUnit.GetComponent<Agent>();
        agent.StartCoroutine(WaitUntilReached());

    }

    private System.Collections.IEnumerator WaitUntilReached()
    {

        yield return new WaitForSeconds(1.5f); //Delay timer between moves

        complete = true;
        Debug.Log("HA RESUCITADO");
        GameManager.Instance.unitsUsed += 1;
        if (GameManager.Instance.unitsUsed >= GameManager.Instance.unitsPerTurn)
        {
            GameManager.Instance.EndAITurn();
        }
        else GameManager.Instance.GoapAgent.TurnStart();



    }

}

//MOVE TO DEAD ALLY IN ORDER TO RESURRECT LATER
public class MoveToDeadAllyStrategy : IActionStrategy
{
    readonly Unit currentUnit;
    readonly Unit deadAlly;
    bool complete;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public MoveToDeadAllyStrategy(Unit currentUnit, Unit deadAlly)
    {
        this.currentUnit = currentUnit;
        this.deadAlly = deadAlly;
    }

    public void Start()
    {
        GameManager.Instance.GoapAgent.interacting = true; //Asi no se calculan mas goals mientras se juega
        Debug.Log("JOER QUIERO RESUCITAR TIO");
        // First step: Find nearby enemies
        Unit[] nearbyEnemies = currentUnit.GetComponent<Agent>().EnemiesAvailable();

        //POSSIBILITY: Instead of goap agent having a target enemy per default, calculate the target enemy in this step

        // Second step: Find the best safe places considering all nearby enemies
        GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());

        Agent agent = currentUnit.GetComponent<Agent>();

        if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
        {
            float recordDistance = Mathf.Infinity;
            int count = 0;
            
            for (int i = 0; i < bestSafePlaces.Length; i++)
            {
                
                float distance = agent.DistanceBetweenTwoNodes(bestSafePlaces[i].worldPosition, deadAlly.transform.position);
                //Debug.Log("DISTANCIA: " + distance);
                if (distance < recordDistance)
                {
                    recordDistance = distance;
                    count = i;

                }
        
            }


            Vector3 targetPosition = bestSafePlaces[count].worldPosition;
            agent.GoTo(targetPosition);

            //currentUnit.GetComponent<Agent>().GoTo(bestSafePlaces[count].worldPosition);
            agent.StartCoroutine(WaitUntilReached());

        }

        
    }

    private System.Collections.IEnumerator WaitUntilReached()
    {

        yield return new WaitForSeconds(1.5f); //Delay timer between moves

        complete = true;
        Debug.Log("SE MUEVE HACIA RESUCITADO");
        GameManager.Instance.unitsUsed += 1;
        if (GameManager.Instance.unitsUsed >= GameManager.Instance.unitsPerTurn)
        {
            GameManager.Instance.EndAITurn();
        }
        else GameManager.Instance.GoapAgent.TurnStart();



    }


}


//FLEE FROM ENEMY DIRECTION
public class FleeFromEnemyStrategy : IActionStrategy
{
    readonly Unit currentUnit;
    readonly Unit targetEnemy;
    bool complete;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public FleeFromEnemyStrategy(Unit currentUnit, Unit targetEnemy)
    {
        this.currentUnit = currentUnit;
        this.targetEnemy = targetEnemy;
    }

    public void Start()
    {
        GameManager.Instance.GoapAgent.interacting = true; //Asi no se calculan mas goals mientras se juega
        // First step: Find nearby enemies
        Unit[] nearbyEnemies = currentUnit.GetComponent<Agent>().EnemiesAvailable();

        //POSSIBILITY: Instead of goap agent having a target enemy per default, calculate the target enemy in this step

        // Second step: Find the best safe places considering all nearby enemies
        GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());
        Agent agent = currentUnit.GetComponent<Agent>();

        if (bestSafePlaces.Length > 0) //moves to the safest place farest to the target enemy (flees from him)
        {
            float maxDistance = 0;
            int count = 0;
            
            for (int i = 0; i < bestSafePlaces.Length; i++)
            {
                float distance = agent.DistanceBetweenTwoNodes(bestSafePlaces[i].worldPosition, targetEnemy.transform.position);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    count = i;

                }
            }

            Vector3 targetPosition = bestSafePlaces[count].worldPosition;

            agent.GoTo(targetPosition);
            agent.StartCoroutine(WaitUntilReached());


        }
    }

    private System.Collections.IEnumerator WaitUntilReached()
    {

        yield return new WaitForSeconds(1.5f); //Delay timer between moves

        complete = true;
        Debug.Log("HUYENDO");
        GameManager.Instance.unitsUsed += 1;
        if (GameManager.Instance.unitsUsed >= GameManager.Instance.unitsPerTurn)
        {
            GameManager.Instance.EndAITurn();
        }
        else GameManager.Instance.GoapAgent.TurnStart();



    }


}

//ATTACK ENEMY TARGET
public class AttackStrategy : IActionStrategy
{
    readonly Unit currentUnit;
    readonly Unit targetEnemy;
    bool complete;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public AttackStrategy(Unit currentUnit, Unit targetEnemy)
    {
        this.currentUnit = currentUnit;
        this.targetEnemy = targetEnemy;
    }

    public void Start()
    {
        Debug.Log("ATACO EPICAMENTE");
        Agent agent = currentUnit.GetComponent<Agent>();
        GameManager.Instance.GoapAgent.interacting = true; //Asi no se calculan mas goals mientras se juega
        currentUnit.AIAttack(targetEnemy); //attacks
        agent.StartCoroutine(WaitUntilReached());
 
    }

    public void Update(float deltaTime) { }

    public void Stop() { }

    private System.Collections.IEnumerator WaitUntilReached()
    {

        yield return new WaitForSeconds(1.5f); //Delay timer between moves

        complete = true;
        Debug.Log("HA ATACADO");
        GameManager.Instance.unitsUsed += 1;
        if (GameManager.Instance.unitsUsed >= GameManager.Instance.unitsPerTurn)
        {
            GameManager.Instance.EndAITurn();
        }
        else GameManager.Instance.GoapAgent.TurnStart();



    }

}


/*
public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination)
    {
        this.agent = agent;
        this.destination = destination;
    }

    public void Start() => agent.SetDestination(destination());
    public void Stop() => agent.ResetPath();
}

public class WanderStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly float wanderRadius;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius)
    {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}

public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent can always Idle
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public IdleStrategy(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}
*/