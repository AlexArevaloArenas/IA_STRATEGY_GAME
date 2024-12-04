using System;
using System.Collections.Generic;
using System.Linq;
using DependencyInjection; // https://github.com/adammyhre/Unity-Dependency-Injection-Lite
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.VolumeComponent;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
public class GoapAgent : MonoBehaviour
{
    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor;
    [SerializeField] Sensor attackSensor;

    [Header("Known Locations")]
    [SerializeField] Transform restingPosition;
    [SerializeField] Transform foodShack;
    [SerializeField] Transform doorOnePosition;
    [SerializeField] Transform doorTwoPosition;

    NavMeshAgent navMeshAgent;
    AnimationController animations;
    Rigidbody rb;

    [Header("Unit Data")]
    public float health = 100;
    public float stamina = 100;

    CountdownTimer statsTimer;

    Unit currentUnit;
    Unit enemyUnit;

    List<Unit> allies;
    List<Unit> enemies;

    GameObject target;
    Vector3 destination;

    AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan;
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    [Inject] GoapFactory gFactory;
    IGoapPlanner gPlanner;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gPlanner = gFactory.CreatePlanner();
    }

    void Start()
    {
        SetupGOAP();
        
    }

    //This function is called EVERY TURN AI PLAYS 
    public void SetupGOAP(){

        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
        
        allies = GameManager.Instance.enemyTeam.ToList(); //allies = AI (enemy team)
        enemies = GameManager.Instance.playerTeam.ToList(); //enemies = Player (player team)
        enemyUnit = SelectMostDangerousUnit(allies, enemies);
        currentUnit = SelectCurrentUnit(enemyUnit, allies);
        
    }

    void SetupBeliefs()
    {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        /*
        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);
        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddBelief("AgentHealthLow", () => health < 30);
        factory.AddBelief("AgentIsHealthy", () => health >= 50);
        factory.AddBelief("AgentStaminaLow", () => stamina < 10);
        factory.AddBelief("AgentIsRested", () => stamina >= 50);

        factory.AddLocationBelief("AgentAtDoorOne", 3f, doorOnePosition);
        factory.AddLocationBelief("AgentAtDoorTwo", 3f, doorTwoPosition);
        factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);
        factory.AddLocationBelief("AgentAtFoodShack", 3f, foodShack);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
        factory.AddBelief("AttackingPlayer", () => false); // Player can always be attacked, this will never become true
        */

        // New beliefs
        factory.AddBelief("CanMoveToAttackPosition", () => currentUnit.GetComponent<Agent>().HasPath());
        factory.AddBelief("CanAttackEnemy", () => enemyUnit != null && Vector3.Distance(currentUnit.transform.position, enemyUnit.transform.position) <= currentUnit.AttackRange);
    }

    void SetupActions()
    {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"])
            .Build());

            

        actions.Add(new AgentAction.Builder("MoveToAttackPosition")
            .WithStrategy(new MoveToAttackPositionStrategy(currentUnit, enemyUnit))
            .AddPrecondition(beliefs["PlayerInChaseRange"])
            //.AddEffect(beliefs["PlayerInAttackRange"])
            .AddEffect(beliefs["CanMoveToAttackPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("AttackPlayer")
            .WithStrategy(new AttackStrategy(currentUnit))
            //.AddPrecondition(beliefs["PlayerInAttackRange"])
            .AddPrecondition(beliefs["CanMoveToAttackPosition"])
            //.AddEffect(beliefs["AttackingPlayer"])
            .AddEffect(beliefs["CanAttackEnemy"])
            .Build());
            

        
    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("AttackEnemyTarget")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["CanAttackEnemy"])
            .Build());
    }

    void SetupTimers()
    {
        statsTimer = new CountdownTimer(2f);
        statsTimer.OnTimerStop += () => {
            UpdateStats();
            statsTimer.Start();
        };
        statsTimer.Start();
    }

    // TODO move to stats system
    void UpdateStats()
    {
        stamina += InRangeOf(restingPosition.position, 3f) ? 20 : -10;
        health += InRangeOf(foodShack.position, 3f) ? 20 : -5;
        stamina = Mathf.Clamp(stamina, 0, 100);
        health = Mathf.Clamp(health, 0, 100);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(currentUnit.transform.position, pos) < range;

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    void HandleTargetChanged()
    {
        Debug.Log("Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }

    void Update(){
        statsTimer.Tick(Time.deltaTime);

        // Update the plan and current action if there is one
        if (currentAction == null)
        {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0)
            {
                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in plan");
                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {currentAction.Name}");
                // Verify all precondition effects are true
                if (currentAction.Preconditions.All(b => b.Evaluate()))
                {
                    currentAction.Start();
                }
                else
                {
                    Debug.Log("Preconditions not met, clearing current action and goal");
                    currentAction = null;
                    currentGoal = null;
                }
            }

            
        }

            // If we have a current action, execute it
        if (actionPlan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete)
            {
                Debug.Log($"{currentAction.Name} complete");
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0)
                {
                    Debug.Log("Plan complete");
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    /* UPDATE WITH NAVMESH AGENT
    void Update()
    {
        statsTimer.Tick(Time.deltaTime);
        //animations.SetSpeed(navMeshAgent.velocity.magnitude);

        // Update the plan and current action if there is one
        if (currentAction == null)
        {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0)
            {
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in plan");
                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {currentAction.Name}");
                // Verify all precondition effects are true
                if (currentAction.Preconditions.All(b => b.Evaluate()))
                {
                    currentAction.Start();
                }
                else
                {
                    Debug.Log("Preconditions not met, clearing current action and goal");
                    currentAction = null;
                    currentGoal = null;
                }
            }
        }

        // If we have a current action, execute it
        if (actionPlan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete)
            {
                Debug.Log($"{currentAction.Name} complete");
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0)
                {
                    Debug.Log("Plan complete");
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    */

    void CalculatePlan()
    {
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        // If we have a current goal, we only want to check goals with higher priority
        if (currentGoal != null)
        {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
        }

        var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null)
        {
            actionPlan = potentialPlan;
        }
    }

    private Unit SelectMostDangerousUnit(List<Unit> allies, List<Unit> enemies)
    {
        if (enemies == null || enemies.Count == 0)
        {
            return null;
        }

        bool isAggressive = allies.Count < enemies.Count; // If we have less allies than enemies, AI plays agressive

        if (isAggressive)
        {
            // Select the enemy with the highest damage and lowest health
            return enemies.OrderByDescending(e => e.AttackDamage)
                          .ThenBy(e => e.GetHealth())
                          .FirstOrDefault();
        }
        else
        {
            // Select the enemy with less than 50% health, or the one with the highest health and damage
            return enemies.OrderBy(e => e.GetHealth() < e.MaxHealth * 0.5f ? 0 : 1)
                          .ThenBy(e => e.GetHealth() < e.MaxHealth * 0.5f ? e.GetHealth() : -e.GetHealth())
                          .ThenByDescending(e => e.AttackDamage)
                          .FirstOrDefault();
        }
    }

    private Unit SelectCurrentUnit(Unit currentEnemy, List<Unit> allies)
    {

        List<Unit> strongestUnits = new List<Unit>();
        Unit selectedUnit = null;

        switch(currentEnemy.type){

            case UnitType.Archer:
                strongestUnits = allies.Where(u => u.type == UnitType.Knight).ToList();
                break;

            case UnitType.Mage:
                strongestUnits = allies.Where(u => u.type == UnitType.Archer).ToList();
                break;

            case UnitType.Knight:
                strongestUnits = allies.Where(u => u.type == UnitType.Mage).ToList();
                break;

            default:
                strongestUnits = allies.Where(u => u.type == UnitType.Pawn).ToList();
                break;
        }
            selectedUnit = CalculateViableEnemy(strongestUnits, currentEnemy);

            // If no strongest unit is found, select a Pawn
            if (selectedUnit == null)
            {
                selectedUnit = allies
                    .Where(u => u.type == UnitType.Pawn)
                    .OrderBy(u => Vector3.Distance(u.transform.position, currentEnemy.transform.position))
                    .FirstOrDefault();
            }

            // If no Pawn is found, select the closest weaker unit
            if (selectedUnit == null)
            {
                selectedUnit = allies
                    .OrderBy(u => Vector3.Distance(u.transform.position, currentEnemy.transform.position))
                    .FirstOrDefault();
            }
            
        
        return selectedUnit;
    }

    private Unit CalculateViableEnemy(List<Unit> allies, Unit currentEnemy){
        
        // Select the strongest unit that has more than 40% health
        Unit selectedUnit = allies.Where(u => u.GetHealth() > u.MaxHealth * 0.4f)
                                  .OrderBy(u => Vector3.Distance(u.transform.position, currentEnemy.transform.position))
                                  .FirstOrDefault();

        return selectedUnit;
    }


}