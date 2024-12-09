using FischlWorks_FogWar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject spawnerPlayer;
    public GameObject spawnerEnemy;
    public GameObject[] availableUnits;

    int state = 0;
    public List<Unit> playerTeam;
    public List<Unit> enemyTeam;

    public List<Unit> visibleAlivePlayerTeam;
    public List<Unit> visibleAliveEnemyTeam;

    public bool acabado = false;
    bool victory = false;
    public bool isPlayerTurn;
    public bool isEnemyTurn;
    public int unitsPerTurn;
    public int unitsUsed;
    int teamSize;
    int killCount;

    public int PlayerBlood = 0;
    public int AIBlood = 0;

    public GoapAgent GoapAgent;

    public GameObject canvasPrefab;
    public csFogWar fogWar = null;
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else if (this != Instance)
        {
            Destroy(gameObject);
        }

    }


    void Start() {
        Time.timeScale = 1;
        
        try
        {
            fogWar = GameObject.Find("FogWar").GetComponent<csFogWar>();
            GoapAgent = GameObject.Find("GOAP").GetComponent<GoapAgent>();
            //GoapAgent.enabled = true;

        }
        catch
        {
            Debug.LogErrorFormat("Failed to fetch csFogWar component. " +
                "Please rename the gameobject that the module is attachted to as \"FogWar\", " +
                "or change the implementation located in the csFogVisibilityAgent.cs script.");
        }
        GeneraEquipos();
    }

    void Update() {

        switch (state) {
            case 0: startGame(); break;
            case 1: runGame(); break;
            case 2: endGame(); break;
        }
    }


    void startGame() {
        
        /*
        if (creación de personajes completa){ 
        if (Random.Range(0,1) == 0) isPlayerTurn = true;
        else isPlayerTurn = false;
        unitUsed = 0;
        }
        */
        state = 1;
    }

    void runGame() {
        if (acabado && victory == false){
            Debug.Log("Jugador es un manco");
            //Jugador Pierde
            Time.timeScale = 0;
            //Mostrar cosas del canvas
        }
        
        else if (acabado && victory){
            Debug.Log("buena tío");
            //Jugador Gana
            Time.timeScale = 0;
            //Mostrar cosas del canvas
        }

        if (isPlayerTurn){
            //el jugador puede seleccionar lo que quiera, hay que incluir que al seleccionar el ataque de una unidad no se pueda seleccionar ya, y se haga un unitsUsed++

        } 
        else {
            //que la Ia haga sus cosas de GOAP e historias, cuando haga el ataque con una unidad, hay que hacer que no pueda hacer nada con esa en este turno, y que se haga un unitsUsed++
            if (isEnemyTurn == false)
            {
                GoapAgent.TurnStart();
                isEnemyTurn = true;
            }
            
        }
           
    }

    void endGame() {
        
        if (victory) {
            //canvasPrefab.GetComponent<Menu>().YouWin();
        }
        else {
            //canvasPrefab.GetComponent<Menu>().YouDied();
        }
        Time.timeScale = 0;
    }

    void GeneraEquipos()
    {
        /*
        int r = -1;
        for (int i = 0; i<teamSize;i++){
            if (i >teamSize/2 && teamSize > 3){
                //playerTeam[i] = infanteria
                //enemyTeam[i] = infanteria
            }
            else{
                r = Random.Range(0,2);
                switch (r) {
                    case 0: {
                        //playerTeam[i] = mago

                        break;
                    } 
                    case 1: {
                        //playerTeam[i] = caballero
                        break;
                    } 
                    case 2: {
                        //playerTeam[i] = arquero
                        break;
                    } 
                }
                r = Random.Range(0,2);
                switch (r) {
                    case 0: {
                        //enemyTeam[i] = mago

                        break;
                    } 
                    case 1: {
                        //enemyTeam[i] = caballero
                        break;
                    } 
                    case 2: {
                        //enemyTeam[i] = arquero
                        break;
                    }
                }
            }
            //playerTeam[i].position = ;
            //enemyTeam[i].position = ;
        }
        */
        for (int i = 0; i < 5; i++) {
            GameObject unit1 = availableUnits[Random.Range(0, 4)];
            GameObject punit = Instantiate(unit1, spawnerPlayer.transform.GetChild(i).transform.position, Quaternion.identity);
            playerTeam.Add(punit.GetComponent<Unit>());
        }
        for (int i = 0; i < 5; i++)
        {
            GameObject unit2 = availableUnits[Random.Range(4, 8)];
            GameObject eunit = Instantiate(unit2, spawnerEnemy.transform.GetChild(i).transform.position, Quaternion.identity);
            enemyTeam.Add(eunit.GetComponent<Unit>());
        }
        
        foreach (Unit u in playerTeam)
        {
            //Debug.Log("AAAAAAAAAAAAAAA");
            fogWar.AddFogExternal(u);
            //Debug.Log(":((((((((((((((");
        }

        //QUITAR CUANDO NO QUERAMOS VER A LOS ENEMIGOS
        foreach (Unit u in enemyTeam)
        {
            //Debug.Log("AAAAAAAAAAAAAAA");
            fogWar.AddFogExternal(u);
            //Debug.Log(":((((((((((((((");
        }
        TeamCheck();
    }    

    public void EndUnitAction()
    {
        unitsUsed += 1;
        if (unitsUsed >= unitsPerTurn)
        {
            isPlayerTurn = !isPlayerTurn;
            unitsUsed = 0;
        }
    }

    public void TeamCheck()
    {
        killCount = 0;
        visibleAlivePlayerTeam.Clear();
        visibleAliveEnemyTeam.Clear();
        //&& u.visible
        foreach (Unit u in playerTeam) {
            if (u.currentHealth>0 && u.visible)
            {
                visibleAlivePlayerTeam.Add(u);
            } 
            if (u.currentHealth<0) killCount++;
        }
        if (killCount >= playerTeam.Count) acabado = true;
         
        foreach (Unit u in enemyTeam) {
            if (u.currentHealth > 0)
            {
                visibleAliveEnemyTeam.Add(u);
            }            
        }
        if (visibleAliveEnemyTeam.Count <= 0){
            victory = true;
            acabado = true;
        }

        
    }

    public void EndAITurn()
    {
        isPlayerTurn=true;
        isEnemyTurn=false;
        unitsUsed = 0;
    }

}