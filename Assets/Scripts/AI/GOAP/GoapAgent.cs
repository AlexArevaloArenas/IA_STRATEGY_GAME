using System;
using System.Collections.Generic;
using System.Linq;
using DependencyInjection; // https://github.com/adammyhre/Unity-Dependency-Injection-Lite
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.VolumeComponent;
using UnityServiceLocator; // https://github.com/adammyhre/Unity-Service-Locator

public class GoapAgent : MonoBehaviour
{
    /*
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
    */

    CountdownTimer statsTimer;


    public bool AITurn;
    public bool interacting;
    Unit currentUnit;
    Unit enemyUnit;
    Unit deadUnit;

    List<Unit> allies;
    List<Unit> enemies;

    List<Unit> playedUnits;

    bool fleeEnemy;

    //Mecanica resureccion
    bool canResurrect;
    GameObject resurrectedUnit;
    public GameObject knightPrefab;
    public GameObject archerPrefab;
    public GameObject magePrefab;
    public GameObject pawnPrefab;

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
        /*
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        */


        // Create the planner
        gFactory = GetComponent<GoapFactory>();
        gPlanner = gFactory.CreatePlanner();

    }

    void Start()
    {
        allies = GameManager.Instance.visibleAliveEnemyTeam;
        enemies = GameManager.Instance.visibleAlivePlayerTeam;
        playedUnits = new List<Unit>();
        SetupGOAP();
        AITurn = false;
        
        
    }
 
    void SetupGOAP(){ //Adds one single time to belief, action and goal list (Start from GOAP script)

        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
        
    }

    public void TurnStart()     //This function is called EVERY TURN AI PLAYS
    {

        //allies = GameManager.Instance.enemyTeam.ToList(); //allies = AI (enemy team)
        //enemies = GameManager.Instance.playerTeam.ToList(); //enemies = Player (player team)
        //allies = GameManager.Instance.visibleAliveEnemyTeam;
        //enemies = GameManager.Instance.visibleAlivePlayerTeam;
        allies = GameManager.Instance.enemyTeam;
        enemies = GameManager.Instance.playerTeam;
        canResurrect = CanResurrect(allies);
        if (!canResurrect)
        {
            enemyUnit = SelectMostDangerousUnit(allies, enemies); //Se tiene en cuenta solo las unidades visibles
            currentUnit = SelectCurrentUnit(enemyUnit, allies); //se selecciona cualquier unidad de la IA (no solo visibles)
            fleeEnemy = FleeFromEnemy(currentUnit, enemyUnit);
            Debug.Log("Enemy unit: " + enemyUnit);
            Debug.Log("Current unit: " + currentUnit);
            Debug.Log("Flee enemy: " + fleeEnemy);
        }

        else {
            resurrectedUnit = SelectResurrectType(allies, enemies);
            currentUnit = SelectCurrentUnit(enemyUnit, GameManager.Instance.enemyTeam); //se selecciona cualquier unidad de la IA (no solo visibles)
        } 

        ClearGOAP(); //Preferiblemente llamar a esta funcion cuando se acaba el turno de la IA
        SetupGOAP();
        AITurn = true;
        interacting = false;
        

    }

    public void EndTurn(){
        playedUnits.Clear();
        GameManager.Instance.EndAITurn();

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

        factory.AddBelief("Nothing", () => false);

        factory.AddBelief("Explore", () => enemies.Count == 0);

        //No llega el raycast de ataque al enemigo
        factory.AddBelief("CanMoveToEnemy", () => enemyUnit != null && currentUnit!= null && 
        !Physics.Raycast(currentUnit.transform.position, (enemyUnit.transform.position - currentUnit.transform.position).normalized, 
            currentUnit.AttackRange, LayerMask.NameToLayer("Unit")));

        //Llega el raycast de ataque al enemigo
        factory.AddBelief("CanAttackEnemy", () => enemyUnit != null && currentUnit != null &&
        Physics.Raycast(currentUnit.transform.position, (enemyUnit.transform.position - currentUnit.transform.position).normalized,
            currentUnit.AttackRange, LayerMask.NameToLayer("Unit")));

        //Flee belief
        factory.AddBelief("FleeFromEnemy", () => fleeEnemy);

        //Resurrection beliefs
        factory.AddBelief("CanResurrect", () => canResurrect &&
        Physics.Raycast(currentUnit.transform.position, (deadUnit.transform.position - currentUnit.transform.position).normalized,
            currentUnit.AttackRange, LayerMask.NameToLayer("Unit"))
        );
        factory.AddBelief("MoveToDeadAlly", () => canResurrect &&
        !Physics.Raycast(currentUnit.transform.position, (deadUnit.transform.position - currentUnit.transform.position).normalized,
            currentUnit.AttackRange, LayerMask.NameToLayer("Unit"))
        );




        //factory.AddBelief("CanMoveToAttackPosition", () => currentUnit.GetComponent<Agent>().HasPath());

    }

    void SetupActions()
    {
        actions = new HashSet<AgentAction>();


        /*
        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["Nothing"])
            .Build());
        */

        
        actions.Add(new AgentAction.Builder("Explore")
            .WithStrategy(new ExploreStrategy(currentUnit, enemyUnit))
            .AddPrecondition(beliefs["Explore"])
            //.AddEffect(beliefs["CanMoveToEnemy"])
            .Build());
        

        actions.Add(new AgentAction.Builder("MoveToEnemy")
        .WithStrategy(new MoveToEnemyStrategy(currentUnit, enemyUnit))
        .AddPrecondition(beliefs["CanMoveToEnemy"])
        .AddEffect(beliefs["CanAttackEnemy"]) // Ensure the effect is correctly set
        .Build());

        actions.Add(new AgentAction.Builder("AttackPlayer")
            .WithStrategy(new AttackStrategy(currentUnit, enemyUnit))
            .AddPrecondition(beliefs["CanAttackEnemy"])
            //.AddPrecondition(beliefs["CanMoveToAttackPosition"])
            //.AddEffect(beliefs["AttackingPlayer"])
            //.AddEffect(beliefs["CanAttackEnemy"])
            .Build());

        actions.Add(new AgentAction.Builder("FleeFromEnemy")
            .WithStrategy(new FleeFromEnemyStrategy(currentUnit, enemyUnit))
            .AddPrecondition(beliefs["FleeFromEnemy"])
            //.AddEffect(beliefs["CanMoveToEnemy"])
            .Build());

        
        actions.Add(new AgentAction.Builder("CanResurrect")
            .WithStrategy(new ResurrectStrategy(currentUnit, deadUnit, resurrectedUnit)) //Instancia el prefab de la unidad que elige la IA
            .AddPrecondition(beliefs["CanResurrect"])
            //.AddEffect(beliefs["CanMoveToEnemy"])
            .Build());
        

        actions.Add(new AgentAction.Builder("MoveToDeadAlly")
            .WithStrategy(new MoveToDeadAllyStrategy(currentUnit, deadUnit))
            .AddPrecondition(beliefs["MoveToDeadAlly"])
            //.AddEffect(beliefs["CanMoveToEnemy"])
            .Build());
        


    }

    void SetupGoals()
    {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("FleeFromEnemyTarget") //Flee: Priority 4
           .WithPriority(6)
           .WithDesiredEffect(beliefs["FleeFromEnemy"])
           .Build());

        goals.Add(new AgentGoal.Builder("MoveToDeadAlly") //Resurrect: Priority 3
            .WithPriority(5)
            .WithDesiredEffect(beliefs["MoveToDeadAlly"])
            .Build());


        goals.Add(new AgentGoal.Builder("CanResurrect")
            .WithPriority(4)
            .WithDesiredEffect(beliefs["CanResurrect"])
            .Build());


        goals.Add(new AgentGoal.Builder("AttackEnemyTarget") //Attack: Priority 2
            .WithPriority(3)
            .WithDesiredEffect(beliefs["CanAttackEnemy"])
            .Build());


        goals.Add(new AgentGoal.Builder("MoveToEnemy")
        .WithPriority(2)
        .WithDesiredEffect(beliefs["CanAttackEnemy"]) // Ensure the desired effect matches the action's effect
        .Build());

        goals.Add(new AgentGoal.Builder("Explore") //Explore: Priority 0
            .WithPriority(1)
            .WithDesiredEffect(beliefs["Explore"])
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
        /*
        stamina += InRangeOf(restingPosition.position, 3f) ? 20 : -10;
        health += InRangeOf(foodShack.position, 3f) ? 20 : -5;
        stamina = Mathf.Clamp(stamina, 0, 100);
        health = Mathf.Clamp(health, 0, 100);
        */
    }

    bool CanResurrect(List<Unit> allies)
    {
        if (GameManager.Instance.AIBlood == 0) return false;

        foreach(Unit a in allies)
        {
            if (a.currentHealth <= 0f) {

                deadUnit = a;
                return true;

            } 
        }
        return false;
    }

    bool FleeFromEnemy(Unit currentUnit, Unit targetEnemy)
    {
        if (targetEnemy == null) return false;
        if (currentUnit == null) return false;

        //Debug.Log("Current unit is ", currentUnit);
        Unit counterEnemy = null;
        Unit[] nearbyEnemies = currentUnit.GetComponent<Agent>().EnemiesAvailable();
        if(nearbyEnemies == null) return false;

        Debug.Log(nearbyEnemies);

        GridNode[] bestAttackPlaces = currentUnit.GetComponent<Agent>().FindBestAttackPlaces(
            targetEnemy,
            new List<Unit>(nearbyEnemies),
            currentUnit.type
        );

        switch (currentUnit.type)
        {
            case UnitType.Knight:

                foreach (Unit u in nearbyEnemies)
                {
                    if (u.type == UnitType.Mage)
                    {
                        counterEnemy = u; break;
                    }
                }
                break;

            case UnitType.Mage:

                foreach (Unit u in nearbyEnemies)
                {
                    if (u.type == UnitType.Archer)
                    {
                        counterEnemy = u; break;
                    }
                }
                break;

            case UnitType.Archer:

                foreach (Unit u in nearbyEnemies)
                {
                    if (u.type == UnitType.Knight)
                    {
                        counterEnemy = u; break;
                    }
                }
                break;

            default: break;

        }

        if (counterEnemy != null)
        {
            foreach (GridNode gn in bestAttackPlaces)
            {
                if (Physics.Raycast(counterEnemy.transform.position,
                    (gn.worldPosition - counterEnemy.transform.position).normalized, counterEnemy.AttackRange,
                    LayerMask.NameToLayer("Unit")))
                {
                    return true;


                }
            }
        }

        return false;
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(currentUnit.transform.position, pos) < range;


    /*
    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;



    void HandleTargetChanged()
    {
        Debug.Log("Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }
    */

    void ClearGOAP()
    {
        currentAction = null;
        currentGoal = null;
        beliefs.Clear();
        actions.Clear();
        goals.Clear();
    }

    void Update(){

        AITurn = GameManager.Instance.isEnemyTurn;
        if (!AITurn) return;
        if (interacting) return; //in middle of a strategy we dont want to calculate more

        //statsTimer.Tick(Time.deltaTime);

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

    private Unit SelectMostDangerousUnit(List<Unit> allies, List<Unit> enemies) //Pasar la lista de ENEMIGOS VISIBLES
    {
        /*
        if (enemies == null || enemies.Count == 0) //Para la estrategia de EXPLORAR
        {
            Debug.Log("LISTA JUGADOR VISIBLES ES 0");
            int i = Random.Range(0, GameManager.Instance.playerTeam.Count); //Selecciona una unidad random enemiga para targetear (unidad no visible)
            return GameManager.Instance.playerTeam[i];
        }
        */

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
        /*
        if (enemies.Count == 0) //Vista visible del jugador (lo que ve la IA)
        {
            int i = Random.Range(0, GameManager.Instance.enemyTeam.Count); //Unidad random para jugar 
            return GameManager.Instance.enemyTeam[i];
        }
        */


        List<Unit> strongestUnits = new List<Unit>();
        Unit selectedUnit = null;

        switch(currentEnemy.type)
        {
            case UnitType.Archer:
                strongestUnits = allies.Where(u => u.type == UnitType.Knight && !playedUnits.Contains(u)).ToList();
                break;

            case UnitType.Mage:
                strongestUnits = allies.Where(u => u.type == UnitType.Archer && !playedUnits.Contains(u)).ToList();
                break;

            case UnitType.Knight:
                strongestUnits = allies.Where(u => u.type == UnitType.Mage && !playedUnits.Contains(u)).ToList();
                break;

            default:
                strongestUnits = allies.Where(u => u.type == UnitType.Pawn && !playedUnits.Contains(u)).ToList();
                break;
        }

        selectedUnit = CalculateViableEnemy(strongestUnits, currentEnemy);

        // If no strongest unit is found, select a Pawn
        if (selectedUnit == null)
        {
            selectedUnit = allies.FirstOrDefault(u => u.type == UnitType.Pawn && !playedUnits.Contains(u));
        }

        // If no Pawn is found, select any available unit that hasn't been played
        if (selectedUnit == null)
        {
            selectedUnit = allies.FirstOrDefault(u => !playedUnits.Contains(u));
        }

        // Add the selected unit to the list of played units
        if (selectedUnit != null)
        {
            playedUnits.Add(selectedUnit);
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

    private GameObject SelectResurrectType(List<Unit> allies, List<Unit> enemies)
    {

        List<Unit> knightUnits = new List<Unit>();
        List<Unit> archerUnits = new List<Unit>();
        List<Unit> mageUnits = new List<Unit>();

        List<Unit> auxiliarList = new List<Unit>();

        if (enemies.Count > allies.Count)  //Jugador tiene mas unidades - Se selecciona counter de lo que mas tenga
        { 
            foreach(Unit u in enemies) //Las listas se rellenan con las unidades del jugador
            {
                switch (u.type)
                {
                    case UnitType.Archer:
                        archerUnits.Add(u); break;
                    case UnitType.Knight:
                        knightUnits.Add(u); break;
                    case UnitType.Mage:
                        mageUnits.Add(u); break;

                    default: //Se selecciona una unidad de tipo peon si no hay del resto
                        return pawnPrefab;

                }
            }

            auxiliarList = ObtainUnitList(knightUnits, archerUnits, mageUnits, true); //Se obtiene la lista mas larga (true) y se selecciona el counter
            if (auxiliarList == knightUnits) return magePrefab;
            if (auxiliarList == archerUnits) return knightPrefab;
            if (auxiliarList == mageUnits) return archerPrefab;
            return pawnPrefab;
        }

        else //Si es menor o igual, se hace al reves
        {
            foreach (Unit u in allies) //Las listas se rellenan con las unidades de la IA
            {
                switch (u.type)
                {
                    case UnitType.Archer:
                        archerUnits.Add(u); break;
                    case UnitType.Knight:
                        knightUnits.Add(u); break;
                    case UnitType.Mage:
                        mageUnits.Add(u); break;

                    default: //Se selecciona una unidad de tipo peon si no hay del resto
                        return pawnPrefab;

                }
            }

            auxiliarList = ObtainUnitList(knightUnits, archerUnits, mageUnits, false); //Se obtiene la lista mas corta (false)

            //La unidad resucitada sera del tipo que menos tenga la IA
            if (auxiliarList == knightUnits) return knightPrefab;
            if (auxiliarList == archerUnits) return archerPrefab;
            if (auxiliarList == mageUnits) return magePrefab;
            return pawnPrefab;
        }

    }

    List<Unit> ObtainUnitList(List<Unit> list1, List<Unit> list2, List<Unit> list3, bool larger)
    {
        // Compara las longitudes
        int longitud1 = list1.Count;
        int longitud2 = list2.Count;
        int longitud3 = list3.Count;

        if (larger) //Se busca obtener la lista mas larga
        {
            if (longitud1 >= longitud2 && longitud1 >= longitud3)
            {
                return list1;
            }
            else if (longitud2 >= longitud1 && longitud2 >= longitud3)
            {
                return list2;
            }
            else
            {
                return list3;
            }
        }

        else //Se busca obtener la lista mas corta
        {
            if (longitud1 <= longitud2 && longitud1 <= longitud3)
            {
                return list1;
            }
            else if (longitud2 <= longitud1 && longitud2 <= longitud3)
            {
                return list2;
            }
            else
            {
                return list3;
            }
        }

    }


}