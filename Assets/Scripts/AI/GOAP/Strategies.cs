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
            Vector3 targetPosition = bestAttackPlaces[0].worldPosition;
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

            if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
            {
                float minDistance = Mathf.Infinity;
                int count = 0;
                
                for (int i = 0; i < bestSafePlaces.Length; i++)
                {
                    if (agent == null) Debug.Log("AGENTE HIJO PUTA");
                    if (bestSafePlaces[i].worldPosition == null) Debug.Log("SAFE PLACE HIJO PUTA");
                    if (targetEnemy == null) Debug.Log("TARGET HIJO PUTA");
                    float distance = agent.DistanceBetweenTwoNodes(bestSafePlaces[i].worldPosition, targetEnemy.transform.position);
                    Debug.Log("DISTANCIA: " + distance);
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
        /*
        while (!agent.HasReachedDestination())
        {
            yield return null;
        }
        */

        yield return new WaitForSeconds(3f); //Delay timer between moves

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

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public ExploreStrategy(Unit currentUnit, Unit targetEnemy)
    {
        this.currentUnit = currentUnit;
        this.targetEnemy = targetEnemy;
    }

    public void Start()
    {
        // First step: Find nearby enemies
        Unit[] nearbyEnemies = currentUnit.GetComponent<Agent>().EnemiesAvailable();

        // Second step: Find the best safe places considering all nearby enemies
        GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());

        if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
        {
            float minDistance = Mathf.Infinity;
            int count = 0;

            for (int i = 0; i < bestSafePlaces.Length; i++)
            {
                float distance = Vector3.Distance(bestSafePlaces[i].worldPosition, targetEnemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    count = i;

                }
            }

            currentUnit.GetComponent<Agent>().GoTo(bestSafePlaces[count].worldPosition);

        }
        complete = true;

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
        GameObject.Instantiate(currentUnit, deadAlly.transform.position, Quaternion.identity); //Se instancia la nueva unidad
        GameManager.Instance.enemyTeam.Add(prefab.GetComponent<Unit>()); //Se agrega la nueva unidad a la lista de enemigos
        GameManager.Instance.AIBlood -= 1;
        complete = true;

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
        // First step: Find nearby enemies
        Unit[] nearbyEnemies = currentUnit.GetComponent<Agent>().EnemiesAvailable();

        //POSSIBILITY: Instead of goap agent having a target enemy per default, calculate the target enemy in this step

        // Second step: Find the best safe places considering all nearby enemies
        GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());

        if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
        {
            float maxDistance = 0f;
            int count = 0;

            for (int i = 0; i < bestSafePlaces.Length; i++)
            {
                float distance = Vector3.Distance(bestSafePlaces[i].worldPosition, deadAlly.transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    count = i;

                }
            }

            currentUnit.GetComponent<Agent>().GoTo(bestSafePlaces[count].worldPosition);

        }
        complete = true;
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
        // First step: Find nearby enemies
        Unit[] nearbyEnemies = currentUnit.GetComponent<Agent>().EnemiesAvailable();

        //POSSIBILITY: Instead of goap agent having a target enemy per default, calculate the target enemy in this step

        // Second step: Find the best safe places considering all nearby enemies
        GridNode[] bestSafePlaces = currentUnit.GetComponent<Agent>().FindSafePlaces(nearbyEnemies.ToList());

        if (bestSafePlaces.Length > 0) //moves to the safest place closer to the target enemy
        {
            float maxDistance = 0f;
            int count = 0;

            for (int i = 0; i < bestSafePlaces.Length; i++)
            {
                float distance = Vector3.Distance(bestSafePlaces[i].worldPosition, targetEnemy.transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    count = i;

                }
            }

            currentUnit.GetComponent<Agent>().GoTo(bestSafePlaces[count].worldPosition);

        }
        complete = true;
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
        currentUnit.Attack(targetEnemy); //attacks
        complete = true;
        //TO DO: GameManager next turn
    }

    public void Update(float deltaTime) { }

    public void Stop() { }
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