using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

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

//MOVE TO ATTACK POSITION

public class MoveToAttackPositionStrategy : IActionStrategy
{
    readonly Unit currentUnit;
    readonly Unit targetEnemy;
    bool complete;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public MoveToAttackPositionStrategy(Unit currentUnit, Unit targetEnemy)
    {
        this.currentUnit = currentUnit;
        this.targetEnemy = targetEnemy;
    }

    public void Start()
    {
        GridNode[] bestAttackPlaces = currentUnit.GetComponent<Agent>().FindBestAttackPlaces(
            targetEnemy,
            new List<Unit> { targetEnemy },
            currentUnit.type
        );

        if (bestAttackPlaces.Length > 0) //moves to the best attack place
        {
            currentUnit.GetComponent<Agent>().GoTo(bestAttackPlaces[0].worldPosition);
        }
        else
        {
            complete = true;
        }
    }

    public void Update(float deltaTime)
    {
        if (currentUnit.GetComponent<Agent>().HasReachedDestination())
        {
            complete = true;
        }
    }

    public void Stop()
    {
        complete = true;
    }
}

//ATTACK ENEMY TARGET
public class AttackStrategy : IActionStrategy
{
    readonly Unit currentUnit;
    bool complete;

    public bool CanPerform => !complete;
    public bool Complete => complete;

    public AttackStrategy(Unit currentUnit)
    {
        this.currentUnit = currentUnit;
    }

    public void Start()
    {
        currentUnit.Attack(currentUnit.GetComponent<Agent>().target); //attacks
        complete = true;
        //TO DO: GameManager next turn
    }

    public void Update(float deltaTime) { }

    public void Stop() { }
}




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