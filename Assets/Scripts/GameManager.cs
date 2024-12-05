using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    
    int state = -1;
    public Unit[] playerTeam;
    public Unit[] enemyTeam;
    public bool acabado = false;
    bool victory = false;
    public bool isPlayerTurn;
    [SerializeField] private int unitsPerTurn;
    public int unitsUsed;
    int teamSize;

    public GoapAgent GoapAgent { get; private set; }

    public GameObject canvasPrefab;
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
    }

    void Update() {

        switch (state) {
            case 0: startGame(); break;
            case 1: runGame(); break;
            case 2: endGame(); break;
        }
    }


    void startGame() {
        GeneraEquipos();
        /*
        if (creación de personajes completa){ 
        if (Random.Range(0,1) == 0) isPlayerTurn = true;
        else isPlayerTurn = false;
        unitUsed = 0;
        state = 1;
        }
        */
    }

    void runGame() {
        if (playerTeam.Length == 0){
            //Jugador Pierde
            Time.timeScale = 0;
            //Mostrar cosas del canvas
        }
        
        else if (enemyTeam.Length == 0){
            //Jugador Gana
            Time.timeScale = 0;
            //Mostrar cosas del canvas
        }

        if (isPlayerTurn){
            //el jugador puede seleccionar lo que quiera, hay que incluir que al seleccionar el ataque de una unidad no se pueda seleccionar ya, y se haga un unitsUsed++

        } 
        else {
            //que la Ia haga sus cosas de GOAP e historias, cuando haga el ataque con una unidad, hay que hacer que no pueda hacer nada con esa en este turno, y que se haga un unitsUsed++
            GoapAgent.SetupGOAP();
        }
            
        statCheck();
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


    }    
    
    void statCheck() {

        
        
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

}